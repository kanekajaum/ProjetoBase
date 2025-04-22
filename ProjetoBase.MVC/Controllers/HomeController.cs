using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjetoBase.MVC.Models;
using System.Security.Cryptography;
using System.Text;

namespace ProjetoBase.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7244");
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("/api/Produto/ListarTodos");
            if (!response.IsSuccessStatusCode)
                return View(new List<ProdutoViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var produtos = JsonConvert.DeserializeObject<List<ProdutoViewModel>>(json);

            return View(produtos);
        }

        public IActionResult Cadastrar()
        {
            var auth = ValidarAuth();
            if (auth != null) return auth;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar(ProdutoViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var content = CriarContentComToken(model);
            var response = await _httpClient.PostAsync("/api/Produto/Cadastrar", content);

            if (!response.IsSuccessStatusCode)
                return View(model);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            AdicionarTokenNoHeader();
            var auth = ValidarAuth();
            if (auth != null) return auth;

            var response = await _httpClient.GetAsync($"api/Produto/BuscarPorId?produtoID={id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var produto = await response.Content.ReadFromJsonAsync<ProdutoViewModel>();
            return View(produto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ProdutoViewModel produto)
        {
            if (id != produto.Id) return BadRequest();

            AdicionarTokenNoHeader();

            var response = await _httpClient.PutAsJsonAsync($"/api/Produto/AlterarProduto?id={id}", produto);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError(string.Empty, "Erro ao editar produto");
            return View(produto);
        }

        public async Task<IActionResult> Deletar(int id)
        {
            var auth = ValidarAuth();
            if (auth != null) return auth;

            AdicionarTokenNoHeader();
            var response = await _httpClient.GetAsync($"api/Produto/BuscarPorId?produtoID={id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var produto = await response.Content.ReadFromJsonAsync<ProdutoViewModel>();
            return View(produto);
        }

        [HttpPost, ActionName("Deletar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletarConfirmado(int id)
        {
            AdicionarTokenNoHeader();
            var response = await _httpClient.DeleteAsync($"/api/produto/Remover?id={id}");

            if (!response.IsSuccessStatusCode)
                return BadRequest();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/Usuario/Login", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewData["ErrorMessage"] = "E-mail ou senha inválidos.";
                return View(model);
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenObj = JsonConvert.DeserializeObject<TokenResponse>(json);

            HttpContext.Session.SetString("AuthToken", tokenObj.Token);

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Registrar(LoginViewModel model)
        {
            return View();
        }

        public static string HashSenhaSHA256(string senha)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
                return Convert.ToBase64String(bytes);
            }
        }

        private IActionResult? ValidarAuth()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }
            return null;
        }

        private HttpContent CriarContentComToken<T>(T model)
        {
            var token = HttpContext.Session.GetString("AuthToken");

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var json = JsonConvert.SerializeObject(model);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private void AdicionarTokenNoHeader()
        {
            var token = HttpContext.Session.GetString("AuthToken");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AuthToken");
            HttpContext.Session.Remove("EmailToken");
            return RedirectToAction("Index", "Home");
        }
    }
}
