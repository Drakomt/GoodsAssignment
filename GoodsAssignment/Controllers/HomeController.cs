using Microsoft.AspNetCore.Mvc;
using GoodsAssignment.Models;
using GoodsAssignment.Data;
using GoodsAssignment.Services;
using System.Text.Json;
using System.Reflection;

using System.Text;


namespace GoodsAssignment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly DataBaseContext _dbContext;
        private readonly Auth _auth;

        public HomeController(DataBaseContext dbContext, Auth auth)
        {
            _dbContext = dbContext;
            _auth = auth;
        }



        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel loginModel)
        {
            try
            {
                if(loginModel == null)
                {
                    return BadRequest("Invalid User. No user provided");
                }

                // Validate the provided username and password in the Users table
                User user = _dbContext.Users.FirstOrDefault(u => u.UserName == loginModel.Username && u.Password == loginModel.Password);

                if (user != null && user.Active)
                {
                    // Generate the secret key 
                    string secretKey = _auth.EncryptUser(user);


                    // Return the secret key with a 200 OK response
                    return Ok(secretKey);
                }

                // If no match is found or the user is not active, return 404 NotFound response
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("items")]
        public IActionResult ReadItems(string columnName = null, string filterValue = null, int page = 1, int pageSize = 10)
        {
            try
            {
                // Get the queryable DbSet for the Items table
                IQueryable<Item> query = _dbContext.Items;

                // Apply filtering based on the column name and filter value
                if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(filterValue))
                {
                    // Use reflection to get the property based on the provided column name
                    var propertyInfo = typeof(Item).GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (propertyInfo != null)
                    {
                        // Fetch the data that matches the filter criteria from the database
                        var matchingItems = query.ToList().Where(item =>
                            propertyInfo.GetValue(item)?.ToString()?.ToLower().Contains(filterValue.ToLower()) == true);

                        // Calculate the total number of items for pagination
                        int totalItems = matchingItems.Count();

                        // Apply pagination to the in-memory filtered data
                        var paginatedItems = matchingItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                        // Return the paginated items with the total number of items in the response
                        return Ok(new { Items = paginatedItems, TotalItems = totalItems });
                    }
                    else
                    {
                        // Invalid column name provided
                        return BadRequest("Invalid column name for filtering.");
                    }
                }

                // Calculate the total number of items for pagination
                int totalItemsCount = query.Count();

                // Apply pagination
                query = query.Skip((page - 1) * pageSize).Take(pageSize);

                // Return the paginated items with the total number of items in the response
                return Ok(new { Items = query.ToList(), TotalItems = totalItemsCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("businesspartners")]
        public IActionResult ReadBusinessPartners(string columnName = null, string filterValue = null, int page = 1, int pageSize = 10)
        {
            try
            {
                // Get the queryable DbSet for the BusinessPartners table
                IQueryable<BusinessPartner> query = _dbContext.BusinessPartners;

                // Apply filtering based on the column name and filter value
                if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(filterValue))
                {
                    // Use reflection to get the property based on the provided column name
                    var propertyInfo = typeof(BusinessPartner).GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (propertyInfo != null)
                    {
                        // Fetch the data that matches the filter criteria from the database
                        var matchingBusinessPartners = query.ToList().Where(bp =>
                            propertyInfo.GetValue(bp)?.ToString()?.ToLower().Contains(filterValue.ToLower()) == true);

                        // Calculate the total number of business partners for pagination
                        int totalBusinessPartners = matchingBusinessPartners.Count();

                        // Apply pagination to the in-memory filtered data
                        var paginatedBusinessPartners = matchingBusinessPartners.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                        // Return the paginated business partners with the total number of items in the response
                        return Ok(new { BusinessPartners = paginatedBusinessPartners, TotalBusinessPartners = totalBusinessPartners });
                    }
                    else
                    {
                        // Invalid column name provided
                        return BadRequest("Invalid column name for filtering.");
                    }
                }

                // Calculate the total number of business partners for pagination
                int totalBusinessPartnersCount = query.Count();

                // Apply pagination
                query = query.Skip((page - 1) * pageSize).Take(pageSize);

                // Return the paginated business partners with the total number of items in the response
                return Ok(new { BusinessPartners = query.ToList(), TotalBusinessPartners = totalBusinessPartnersCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        [HttpPost("adddocument")]
        public IActionResult AddDocument([FromBody] object document, [FromHeader] string secretKey)
        {
            try
            {
                // Check if the document is provided
                if (document == null)
                {
                    return BadRequest("Invalid document. No document provided");
                }
                // Check if the secretKey is provided
                if (secretKey == null)
                {
                    return BadRequest("Invalid User. No user provided");
                }

                User user = _auth.DecryptUser(secretKey);

                // Check if there is a user
                if (user == null)
                {
                    return BadRequest("Invalid User. Invalid user provided");
                }


                // Deserialize the incoming document as JsonElement to inspect its properties
                using var docStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(document)));
                using var jsonDocument = JsonDocument.Parse(docStream);
                var rootElement = jsonDocument.RootElement;

                // Check if the document is of type SaleOrder or PurchaseOrder
                if (rootElement.TryGetProperty("SaleOrderLines", out _))
                {
                    SaleOrder saleOrder = JsonSerializer.Deserialize<SaleOrder>(rootElement.GetRawText());

                    //Check if the document is valid
                    if (saleOrder == null)
                    {
                        return BadRequest("Invalid document format.");
                    }

                    // Check if the SaleOrder is for a valid business partner
                    var businessPartner = _dbContext.BusinessPartners.FirstOrDefault(bp => bp.BPCode == saleOrder.BPCode);
                    if (businessPartner == null || !businessPartner.Active)
                    {
                        return BadRequest("Invalid or inactive business partner.");
                    }

                    // Validate document type and business partner type
                    if (businessPartner.BPType == "V")
                    {
                        return BadRequest("Invalid document type for the selected business partner.");
                    }

                    // Check if the SaleOrder has lines
                    if (saleOrder.SaleOrderLines == null || saleOrder.SaleOrderLines.Count == 0)
                    {
                        return BadRequest("Document must have lines.");
                    }

                    // Check if the items in the SaleOrder are active
                    if (saleOrder.SaleOrderLines.Any(line => !_dbContext.Items.Any(item => item.ItemCode == line.ItemCode && item.Active)))
                    {
                        return BadRequest("One or more items in the document are not active.");
                    }
                    // Set the default values for CreateDate, LastUpdateDate, CreatedBy, and LastUpdatedBy
                    saleOrder.CreateDate = DateTime.Now;
                    saleOrder.LastUpdateDate = null;
                    saleOrder.CreatedBy = user.ID;
                    saleOrder.LastUpdatedBy = null;

                    // Add the SaleOrder to the SaleOrders table and save changes
                    _dbContext.SaleOrders.Add(saleOrder);
                    _dbContext.SaveChanges();

                    // Return the created SaleOrder with a 200 - OK response
                    return Ok(saleOrder);
                }
                else if (rootElement.TryGetProperty("PurchaseOrderLines", out _))
                {
                    PurchaseOrder purchaseOrder = JsonSerializer.Deserialize<PurchaseOrder>(rootElement.GetRawText());

                    //Check if the document is valid
                    if (purchaseOrder == null)
                    {
                        return BadRequest("Invalid document format.");
                    }

                    // Check if the PurchaseOrder is for a valid business partner
                    var businessPartner = _dbContext.BusinessPartners.FirstOrDefault(bp => bp.BPCode == purchaseOrder.BPCode);
                    if (businessPartner == null || !businessPartner.Active)
                    {
                        return BadRequest("Invalid or inactive business partner.");
                    }

                    // Validate document type and business partner type
                    if (businessPartner.BPType == "S")
                    {
                        return BadRequest("Invalid document type for the selected business partner.");
                    }

                    // Check if the PurchaseOrder has lines
                    if (purchaseOrder.PurchaseOrderLines == null || purchaseOrder.PurchaseOrderLines.Count == 0)
                    {
                        return BadRequest("Document must have lines.");
                    }

                    // Check if the items in the PurchaseOrder are active
                    if (purchaseOrder.PurchaseOrderLines.Any(line => !_dbContext.Items.Any(item => item.ItemCode == line.ItemCode && item.Active)))
                    {
                        return BadRequest("One or more items in the document are not active.");
                    }

                    // Set the default values for CreateDate, LastUpdateDate, CreatedBy, and LastUpdatedBy
                    purchaseOrder.CreateDate = DateTime.Now;
                    purchaseOrder.LastUpdateDate = null;
                    purchaseOrder.CreatedBy = user.ID;
                    purchaseOrder.LastUpdatedBy = null;

                    // Add the PurchaseOrder to the PurchaseOrders table and save changes
                    _dbContext.PurchaseOrders.Add(purchaseOrder);
                    _dbContext.SaveChanges();

                    // Return the created PurchaseOrder with a 200 - OK response
                    return Ok(purchaseOrder);
                }
                else
                {
                    return BadRequest("Invalid document type.");
                }


            }
            catch (JsonException)
            {
                return BadRequest("Invalid document format.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPost("updatedocument")]
        public IActionResult UpdateDocument([FromBody] object document, [FromHeader] string secretKey)
        {
            try
            {
                // Check if the document is provided
                if (document == null)
                {
                    return BadRequest("Invalid document. No document provided");
                }
                // Check if the secretKey is provided
                if (secretKey == null)
                {
                    return BadRequest("Invalid User. No user provided");
                }

                User user = _auth.DecryptUser(secretKey);

                // Check if there is a user
                if (user == null)
                {
                    return BadRequest("Invalid User. Invalid user provided");
                }


                // Deserialize the incoming document object to JsonElement
                using var docStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(document)));
                using var jsonDocument = JsonDocument.Parse(docStream);
                var rootElement = jsonDocument.RootElement;

                // Check if the document is of type SaleOrder or PurchaseOrder
                if (rootElement.TryGetProperty("SaleOrderLines", out _))
                {
                    SaleOrder saleOrder = JsonSerializer.Deserialize<SaleOrder>(rootElement.GetRawText());

                    //Check if the document is valid
                    if (saleOrder == null)
                    {
                        return BadRequest("Invalid document format.");
                    }

                    // Check if the document exists in the database
                    var existingDocument = _dbContext.Find(saleOrder.GetType(), saleOrder.ID);
                    if (existingDocument == null)
                    {
                        return NotFound("Document not found.");
                    }

                    var businessPartner = _dbContext.BusinessPartners.FirstOrDefault(bp => bp.BPCode == saleOrder.BPCode);
                    if (businessPartner == null || !businessPartner.Active)
                    {
                        return BadRequest("Invalid or inactive business partner.");
                    }

                    // Validate document type and business partner type
                    if (businessPartner.BPType == "V")
                    {
                        return BadRequest("Invalid document type for the selected business partner.");
                    }


                    // Check if the document has lines
                    if (saleOrder.SaleOrderLines == null)
                    {
                        return BadRequest("Document must have lines.");
                    }


                    // Check if the items in the document are active
                    if (saleOrder.SaleOrderLines.Any(line => !_dbContext.Items.Any(item => item.ItemCode == line.ItemCode && item.Active)))
                    {
                        return BadRequest("One or more items in the document are not active.");
                    }


                    // Set the default values for LastUpdateDate and LastUpdatedBy
                    saleOrder.LastUpdateDate = DateTime.Now;
                    saleOrder.LastUpdatedBy = user.ID;
                    _dbContext.Entry(existingDocument).CurrentValues.SetValues(saleOrder);


                    // Update the document in the appropriate table and save changes
                    _dbContext.SaveChanges();

                    // Return the updated document with a 200 - OK response
                    return Ok(existingDocument);
                }
                else if (rootElement.TryGetProperty("PurchaseOrderLines", out _))
                {
                    PurchaseOrder purchaseOrder = JsonSerializer.Deserialize<PurchaseOrder>(rootElement.GetRawText());

                    //Check if the document is valid
                    if (purchaseOrder == null)
                    {
                        return BadRequest("Invalid document format.");
                    }

                    // Check if the document exists in the database
                    var existingDocument = _dbContext.Find(purchaseOrder.GetType(), purchaseOrder.ID);
                    if (existingDocument == null)
                    {
                        return NotFound("Document not found.");
                    }


                    // Check if the PurchaseOrder is for a valid business partner
                    var businessPartner = _dbContext.BusinessPartners.FirstOrDefault(bp => bp.BPCode == purchaseOrder.BPCode);
                    if (businessPartner == null || !businessPartner.Active)
                    {
                        return BadRequest("Invalid or inactive business partner.");
                    }

                    // Validate document type and business partner type
                    if (businessPartner.BPType == "S")
                    {
                        return BadRequest("Invalid document type for the selected business partner.");
                    }


                    // Check if the document has lines
                    if (purchaseOrder.PurchaseOrderLines == null)
                    {
                        return BadRequest("Document must have lines.");
                    }


                    // Check if the items in the document are active
                    if (purchaseOrder.PurchaseOrderLines.Any(line => !_dbContext.Items.Any(item => item.ItemCode == line.ItemCode && item.Active)))
                    {
                        return BadRequest("One or more items in the document are not active.");
                    }


                    // Set the default values for LastUpdateDate and LastUpdatedBy
                    purchaseOrder.LastUpdateDate = DateTime.Now;
                    purchaseOrder.LastUpdatedBy = user.ID;
                    _dbContext.Entry(existingDocument).CurrentValues.SetValues(purchaseOrder);

                    // Update the document in the appropriate table and save changes
                    _dbContext.SaveChanges();

                    // Return the updated document with a 200 - OK response
                    return Ok(existingDocument);
                }
                else
                {
                    return BadRequest("Invalid document type.");
                }

            }
            catch (JsonException)
            {
                return BadRequest("Invalid document format.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }




        [HttpPost("deletedocument")]
        public IActionResult DeleteDocument(int id, string documentType, [FromHeader] string secretKey)
        {
            try
            {
                // Check if the documentType is provided
                if (documentType == null)
                {
                    return BadRequest("Invalid DocumentType. No documentType provided");
                }
                // Check if the id is provided
                if (id == 0)
                {
                    return BadRequest("Invalid Id. No document Id provided");
                }
                // Check if the secretKey is provided
                if (secretKey == null)
                {
                    return BadRequest("Invalid User. No user provided");
                }

                User user = _auth.DecryptUser(secretKey);

                // Check if there is a user
                if (user == null)
                {
                    return BadRequest("Invalid User. Invalid user provided");
                }


                // Check if the document type is either SaleOrder or PurchaseOrder
                if (documentType != "SaleOrder" && documentType != "PurchaseOrder")
                {
                    return BadRequest("Invalid document type.");
                }
                //var targetType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == documentType && (typeof(SaleOrder).IsAssignableFrom(t) || typeof(PurchaseOrder).IsAssignableFrom(t)));
                var targetType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == documentType);

                // Find the existing document in the database based on the document type and ID
                var existingDocument = _dbContext.Find(targetType, id);

                // If the document is not found, return NotFound
                if (existingDocument == null)
                {
                    return NotFound("Document not found.");
                }

                // Remove the document from the appropriate table and save changes
                _dbContext.Remove(existingDocument);
                _dbContext.SaveChanges();

                // Return a success response
                return Ok("Document deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("getdocument")]
        public IActionResult GetDocument(int id, string documentType)
        {
            try
            {
                // Check if the document type is either SaleOrder or PurchaseOrder
                if (documentType != "SaleOrder" && documentType != "PurchaseOrder")
                {
                    return BadRequest("Invalid document type.");
                }

                // Find the existing document in the database based on the document type and ID
                var targetType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == documentType);
                var existingDocument = _dbContext.Find(targetType, id);

                // If the document is not found, return NotFound
                if (existingDocument == null)
                {
                    return NotFound("Document not found.");
                }

                // Prepare the response object with additional fields
                var response = new Dictionary<string, object>();

                if (documentType == "SaleOrder")
                {
                    var saleOrder = (SaleOrder)existingDocument;

                    response.Add("ID", saleOrder.ID);
                    response.Add("BPCode", saleOrder.BPCode);
                    response.Add("CreateDate", saleOrder.CreateDate);
                    response.Add("LastUpdateDate", saleOrder.LastUpdateDate);
                    response.Add("CreatedBy", saleOrder.CreatedBy);
                    response.Add("LastUpdatedBy", saleOrder.LastUpdatedBy);

                    // Get BPName and Active value for BPCode
                    var businessPartner = _dbContext.BusinessPartners.FirstOrDefault(bp => bp.BPCode == saleOrder.BPCode);
                    if (businessPartner != null)
                    {
                        response.Add("BPName", businessPartner.BPName);
                        response.Add("Active", businessPartner.Active);
                    }

                    // Get FullName value for CreatedBy/LastUpdatedBy user id
                    var createdByUser = _dbContext.Users.FirstOrDefault(u => u.ID == saleOrder.CreatedBy);
                    if (createdByUser != null)
                    {
                        response.Add("FullNameCreatedBy", createdByUser.FullName);
                    }
                    var lastUpdatedByUser = _dbContext.Users.FirstOrDefault(u => u.ID == saleOrder.LastUpdatedBy);
                    if (lastUpdatedByUser != null)
                    {
                        response.Add("FullNameLastUpdatedBy", lastUpdatedByUser.FullName);
                    }
                }
                else if (documentType == "PurchaseOrder")
                {
                    var purchaseOrder = (PurchaseOrder)existingDocument;

                    response.Add("ID", purchaseOrder.ID);
                    response.Add("BPCode", purchaseOrder.BPCode);
                    response.Add("CreateDate", purchaseOrder.CreateDate);
                    response.Add("LastUpdateDate", purchaseOrder.LastUpdateDate);
                    response.Add("CreatedBy", purchaseOrder.CreatedBy);
                    response.Add("LastUpdatedBy", purchaseOrder.LastUpdatedBy);

                    // Get BPName and Active value for BPCode
                    var businessPartner = _dbContext.BusinessPartners.FirstOrDefault(bp => bp.BPCode == purchaseOrder.BPCode);
                    if (businessPartner != null)
                    {
                        response.Add("BPName", businessPartner.BPName);
                        response.Add("Active", businessPartner.Active);
                    }

                    // Get FullName value for CreatedBy/LastUpdatedBy user id
                    var createdByUser = _dbContext.Users.FirstOrDefault(u => u.ID == purchaseOrder.CreatedBy);
                    if (createdByUser != null)
                    {
                        response.Add("FullNameCreatedBy", createdByUser.FullName);
                    }
                    var lastUpdatedByUser = _dbContext.Users.FirstOrDefault(u => u.ID == purchaseOrder.LastUpdatedBy);
                    if (lastUpdatedByUser != null)
                    {
                        response.Add("FullNameLastUpdatedBy", lastUpdatedByUser.FullName);
                    }
                }

                // Return the response
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


    }
}
