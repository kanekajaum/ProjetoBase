using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoBase.Domain.Entities
{
    public class Produto
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Preco { get; private set; }

        public Produto(string nome, decimal preco)
        {
            Nome = nome;
            Preco = preco;
        }

        public void AtualizarPreco(decimal novoPreco)
        {
            if (novoPreco < 0) throw new Exception("Preço inválido.");
            Preco = novoPreco;
        }

        public void AtualizarNome(string novoNome)
        {
            if (string.IsNullOrWhiteSpace(novoNome))
                throw new Exception("Nome inválido.");

            Nome = novoNome;
        }
    }
}