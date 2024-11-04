using FI.AtividadeEntrevista.BLL;
using WebAtividadeEntrevista.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FI.AtividadeEntrevista.DML;
using System.Reflection;
using Newtonsoft.Json;
using System.Web.UI.WebControls;

namespace WebAtividadeEntrevista.Controllers
{
    public class ClienteController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Incluir()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Incluir(ClienteModel model, string beneficiariosData)
        {
            BoCliente bo = new BoCliente();
            BoBeneficiarios bb = new BoBeneficiarios();
            
            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                model.Id = bo.Incluir(new Cliente()
                {
                    CEP = model.CEP,
                    Cidade = model.Cidade,
                    Email = model.Email,
                    Estado = model.Estado,
                    Logradouro = model.Logradouro,
                    Nacionalidade = model.Nacionalidade,
                    Nome = model.Nome,
                    Sobrenome = model.Sobrenome,
                    Telefone = model.Telefone,
                    CPF = model.CPF
                });

                if(beneficiariosData != null)
                {
                    // Desserializa os beneficiários e os vincula ao cliente
                    var beneficiarios = JsonConvert.DeserializeObject<List<BeneficiarioModel>>(beneficiariosData);

                    foreach (var beneficiario in beneficiarios)
                    {
                        beneficiario.IdCliente = model.Id; // Atribui o ID do cliente ao beneficiário

                        // Cria uma nova instância de DML.Beneficiario e mapeia os campos
                        var beneficiarioDML = new Beneficiario()
                        {

                            CPF = beneficiario.CPF,
                            Nome = beneficiario.Nome,
                            IdCliente = beneficiario.IdCliente

                        };

                        bb.Incluir(beneficiarioDML); // Método na camada de negócio para incluir beneficiário
                    }
                }
                
                return Json("Cadastro efetuado com sucesso");
            }
        }

        [HttpPost]
        public JsonResult Alterar(ClienteModel model, string beneficiariosData)
        {
            BoCliente bo = new BoCliente();
            BoBeneficiarios bb = new BoBeneficiarios();

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                bo.Alterar(new Cliente()
                {
                    Id = model.Id,
                    CEP = model.CEP,
                    Cidade = model.Cidade,
                    Email = model.Email,
                    Estado = model.Estado,
                    Logradouro = model.Logradouro,
                    Nacionalidade = model.Nacionalidade,
                    Nome = model.Nome,
                    Sobrenome = model.Sobrenome,
                    Telefone = model.Telefone,
                    CPF = model.CPF
                });

                if(beneficiariosData != null)
                {
                    // Desserializa os beneficiários e os vincula ao cliente
                    var beneficiarios = JsonConvert.DeserializeObject<List<BeneficiarioModel>>(beneficiariosData);

                    foreach (var beneficiario in beneficiarios)
                    {
                        beneficiario.IdCliente = model.Id; // Atribui o ID do cliente ao beneficiário

                        // Cria uma nova instância de DML.Beneficiario e mapeia os campos
                        var beneficiarioDML = new Beneficiario()
                        {

                            CPF = beneficiario.CPF,
                            Nome = beneficiario.Nome,
                            IdCliente = beneficiario.IdCliente

                        };

                        if (bb.VerificarExistencia(beneficiario.CPF)){
                            bb.Alterar(beneficiarioDML); // Método na camada de negócio para incluir beneficiário
                        }
                        else
                        {
                            bb.Incluir(beneficiarioDML);
                        }
                        
                    }
                }                

                return Json("Cadastro alterado com sucesso");
            }
        }

        [HttpGet]
        public ActionResult Alterar(long id)
        {
            BoCliente bo = new BoCliente();
            Cliente cliente = bo.Consultar(id);
            Models.ClienteModel model = null;

            if (cliente != null)
            {
                model = new ClienteModel()
                {
                    Id = cliente.Id,
                    CEP = cliente.CEP,
                    Cidade = cliente.Cidade,
                    Email = cliente.Email,
                    Estado = cliente.Estado,
                    Logradouro = cliente.Logradouro,
                    Nacionalidade = cliente.Nacionalidade,
                    Nome = cliente.Nome,
                    Sobrenome = cliente.Sobrenome,
                    Telefone = cliente.Telefone,
                    CPF = cliente.CPF
                };            
            }

            return View(model);
        }

        [HttpPost]
        public JsonResult ClienteList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                int qtd = 0;
                string campo = string.Empty;
                string crescente = string.Empty;
                string[] array = jtSorting.Split(' ');

                if (array.Length > 0)
                    campo = array[0];

                if (array.Length > 1)
                    crescente = array[1];

                List<Cliente> clientes = new BoCliente().Pesquisa(jtStartIndex, jtPageSize, campo, crescente.Equals("ASC", StringComparison.InvariantCultureIgnoreCase), out qtd);

                //Return result to jTable
                return Json(new { Result = "OK", Records = clientes, TotalRecordCount = qtd });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult VerificarCPF(string cpf)
        {
            BoCliente bo = new BoCliente();
            bool cpfExiste = bo.VerificarExistencia(cpf);
            return Json(new { existe = cpfExiste });
        }

        [HttpPost]
        public JsonResult VerificarCpfBeneficiario(string cpf)
        {
            BoBeneficiarios bb = new BoBeneficiarios();
            bool cpfExiste = bb.VerificarExistencia(cpf);
            return Json(new { existe = cpfExiste });
        }

        [HttpGet]
        public ActionResult ObterBeneficiarios(int clienteId)
        {
            BoBeneficiarios bo = new BoBeneficiarios();

            var beneficiarios = bo.PesquisaBeneficiarioPorIdCliente(clienteId)
                .Select(b => new { b.Id, b.CPF, b.Nome })
                .ToList();

            return Json(beneficiarios, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AlterarBeneficiario(BeneficiarioModel model)
        {
            BoBeneficiarios bo = new BoBeneficiarios();

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                bo.Alterar(new Beneficiario()
                {
                    Id = model.Id,
                    CPF = model.CPF,
                    Nome = model.Nome
                    
                });

                return Json("Cadastro alterado com sucesso");
            }
        }

        [HttpPost]
        public JsonResult ExcluirBeneficiario(string cpf)
        {
            BoBeneficiarios bb = new BoBeneficiarios();

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                bb.Excluir(cpf);

                return Json(new { success = true });
            }
        }

        [HttpPost]
        public JsonResult IncluirBeneficiario(BeneficiarioModel model)
        {
            BoBeneficiarios bo = new BoBeneficiarios();

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                model.Id = bo.Incluir(new Beneficiario()
                {
                    CPF = model.CPF,
                    Nome = model.Nome,
                    IdCliente = model.IdCliente

                });

                return Json("Cadastro efetuado com sucesso");
            }
        }
    }
}