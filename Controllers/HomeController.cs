using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_ilcs.Models;
using System.Diagnostics;
using project_ilcs.dto;
using Newtonsoft.Json;
using System.Text;

namespace project_ilcs.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly EnigmaIlcsDbContext _context;
        private readonly HttpClient _client;

        public HomeController(ILogger<HomeController> logger, EnigmaIlcsDbContext enigmaIlcsDbContext)
        {
            _logger = logger;
            _context = enigmaIlcsDbContext;
            _client = new HttpClient();
        }
        #region function for page view
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<ActionResult> Create()
        {
            ViewBag.countries = await GetCountries();
            return View();
        }

        [HttpGet]
        public ActionResult Edit(int? Id)
        {
            ViewBag.id = Id;

            try
            {
                var _transaction = new Transaction();
                _transaction = (_context.Transactions).Where(a => a.Id == Id).FirstOrDefault();

                return View(_transaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region load data
        [HttpPost]
        public ActionResult LoadTransactionData()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                //Paging Size (10,20,50,100)    
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Getting all Transaction data    
                var transactions = from t in _context.Transactions
                                   join p in _context.Products on t.ProductID equals p.ProductID
                                   select new TransactionResponse
                                   {
                                       Id = t.Id,
                                       Country = t.Country,
                                       Harbor = t.Harbor,
                                       Price = t.Price,
                                       TotalTaxPrice = t.TotalTaxPrice,
                                       ProductName = p.ProductName,
                                       Tax = p.Tax
                                   };

                //Search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    transactions = transactions.Where(t => t.Country.Contains(searchValue) );
                }

                //total number of rows count     
                recordsTotal = transactions.Count();
                var data = transactions.Skip(skip).Take(pageSize).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region get data select countries and cities
        public async Task<List<CountryResponse>> GetCountries()
        {
            var response = await _client.GetAsync("https://countriesnow.space/api/v0.1/countries");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(content);
                var countries = new List<CountryResponse>();
                foreach (var item in data.data)
                {
                    countries.Add(new CountryResponse
                    {
                        Name = item.country.ToString(),
                        Iso2 = item.iso2.ToString(),
                    });
                }
                return countries;
            }

            return new List<CountryResponse>();
        }

        [HttpPost("Harbors")]
        public async Task<IActionResult> GetHarbors([FromBody] CountryResponse request)
        {
            var country = request.Name;
            var content = new StringContent(JsonConvert.SerializeObject(new { country = country }), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("https://countriesnow.space/api/v0.1/countries/cities", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<HarborResponse>(responseContent);
                return Ok(apiResponse.Data);
            }

            return NotFound();
        }

        [HttpGet("Products/{productID}")]
        public List<Products> GetProducts(int? productID)
        {
            var list = (from x in _context.Products
                        where x.ProductID == productID
                        select
                          new Products()
                          {
                              ProductID = x.ProductID,
                              ProductName = x.ProductName,
                              ProductDescription = x.ProductDescription,
                              Tax = x.Tax
                          }).Distinct().ToList();

            return list;
        }
        #endregion

        #region
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(Transaction transaction)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid model state");
                return View(transaction);
            }

            try
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"Unable to save changes: {ex.Message}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            }

            return View(transaction);
        }
        #endregion

        #region update data transaction
        [HttpPut("ModifyTransaction/{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] Transaction updatedTransaction)
        {
            if (id != updatedTransaction.Id)
            {
                return BadRequest();
            }

            var existingTransaction = await _context.Transactions.FindAsync(id);

            if (existingTransaction == null)
            {
                return NotFound();
            }

            existingTransaction.Country = updatedTransaction.Country;
            existingTransaction.Harbor = updatedTransaction.Harbor;
            existingTransaction.ProductID = updatedTransaction.ProductID;
            existingTransaction.Price = updatedTransaction.Price;
            existingTransaction.TotalTaxPrice = updatedTransaction.TotalTaxPrice;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Transactions.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        #endregion

        #region delete data transaction
        [HttpDelete("DeleteTransaction/{id}")]
        public JsonResult DeleteTransaction(int? id)
        {
            if (id == null)
            {
                return Json(new { message = "Not Deleted" });
            }

            var transaction = _context.Transactions.Find(id);
            if (transaction == null)
            {
                return Json(new { message = "Transaction not found" });
            }

            _context.Transactions.Remove(transaction);
            _context.SaveChanges();

            return Json(new { message = "Deleted" });
        }
        #endregion

    }
}
