using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebAdvert.Web.Models;
using WebAdvert.Web.Models.Home;
using WebAdvert.Web.ServiceClients;

namespace WebAdvert.Web.Controllers
{
    public class HomeController : Controller
    {
        public ISearchApiClient SearchApiClient { get; }
        public IMapper Mapper { get; }
        public IAdvertApiClient ApiClient { get; }

        public HomeController(ISearchApiClient searchApiClient, IMapper mapper, IAdvertApiClient apiClient)
        {
            SearchApiClient = searchApiClient;
            Mapper = mapper;
            ApiClient = apiClient;
        }

        [Authorize]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> Index()
        {
            IEnumerable<IndexViewModel> allViewModels;
            try
            {
                var allAds = await ApiClient.GetAllAsync();
                allViewModels = allAds.Select(x => Mapper.Map<IndexViewModel>(x));
            }
            catch (Exception)
            {
                allViewModels = new List<IndexViewModel>();
            }
            return View(allViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string keyword)
        {
            var viewModel = new List<SearchViewModel>();

            try
            {
                var searchResult = await SearchApiClient.Search(keyword);
                searchResult.ForEach(advertDoc =>
                {
                    var viewModelItem = Mapper.Map<SearchViewModel>(advertDoc);
                    viewModel.Add(viewModelItem);
                });
            }
            catch (Exception)
            {

            }

            return View("Search", viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
