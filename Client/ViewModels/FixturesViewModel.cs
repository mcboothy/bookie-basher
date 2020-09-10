using BookieBasher.Core.IO;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BookieBasher.Client.ViewModels
{
    public class FixturesViewModel : ComponentBase
    {
        public JSMatch[] Fixtures { get; set; }

        [Inject]
        public HttpClient Http { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Fixtures = await Http.GetFromJsonAsync<JSMatch[]>("Fixtures");
        }
    }
}
