using System;
using System.Collections.Generic;
using System.Linq;

namespace Trabalho_ia_0904
{
    internal class Cidade
    {
        public List<int> Rota { get; set; }
        public int Aptidao { get; set; }

        private static readonly Dictionary<int, string> NomeCidades = new()
        {
            {1, "1"}, {2, "2"}, {3, "3"}, {4, "4"}, {5, "5"},
            {6, "6"}, {7, "7"}, {8, "8"}, {9, "9"}
        };

        private static List<int> estadoFinal = Enumerable.Range(1, 9).ToList();

        public Cidade(List<int> rota)
        {
            Rota = rota;
            Aptidao = CalcularAptidao(estadoFinal);
        }

        public int CalcularAptidao(List<int> estadoFinal)
        {
            int nota = 0;

            for (int i = 0; i < Rota.Count; i++)
            {
                if (i < estadoFinal.Count && Rota[i] != estadoFinal[i])
                    nota += 10;
            }

            for (int i = 0; i < Rota.Count - 1; i++)
            {
                if (Rota[i] > Rota[i + 1])
                    nota += 30;
            }

            int repetidas = Rota.Count - Rota.Distinct().Count();
            if (repetidas > 0)
                nota += 20 * repetidas;

            return nota;
        }

        public static List<Cidade> GerarPopulacao(int tamanho, int numCidades)
        {
            List<Cidade> populacao = new();
            Random random = new();

            for (int i = 0; i < tamanho; i++)
            {
                List<int> rota = Enumerable.Range(1, numCidades)
                                           .OrderBy(x => random.Next())
                                           .ToList();
                populacao.Add(new Cidade(rota));
            }

            return populacao;
        }

        public static void ExibirPopulacao(List<Cidade> populacao, int geracao)
        {
            Console.WriteLine($"\nGeração {geracao}:");
            for (int i = 0; i < populacao.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {populacao[i]}");
            }
        }

        public override string ToString()
        {
            var rotaNomes = Rota.Select(num => NomeCidades[num]).ToList();
            return $"[{string.Join(", ", rotaNomes)}] - {Aptidao}";
        }

        public static List<Cidade> SelecionarPorTorneio(List<Cidade> populacao, int quantidade)
        {
            List<Cidade> selecionados = new();
            Random random = new();

            while (selecionados.Count < quantidade)
            {
                var candidatos = new List<Cidade>();
                while (candidatos.Count < 3)
                {
                    var candidato = populacao[random.Next(populacao.Count)];
                    if (!candidatos.Contains(candidato))
                        candidatos.Add(candidato);
                }

                var melhor = candidatos.OrderBy(c => c.Aptidao).First();
                if (!selecionados.Contains(melhor))
                    selecionados.Add(melhor);
            }

            return selecionados;
        }

        public static List<Cidade> Reproduzir(List<Cidade> populacao, int quantidade, int numCidades)
        {
            List<Cidade> filhos = new();
            Random random = new();

            while (filhos.Count < quantidade)
            {
                var pai = populacao[random.Next(populacao.Count)];
                var mae = populacao[random.Next(populacao.Count)];
                while (mae == pai)
                    mae = populacao[random.Next(populacao.Count)];

                int p1 = random.Next(numCidades - 1);
                int p2 = random.Next(p1 + 1, numCidades);

                var meio = mae.Rota.Skip(p1).Take(p2 - p1).ToList();

                var filhoRota = pai.Rota.Where(c => !meio.Contains(c)).ToList();
                filhoRota.InsertRange(p1, meio);

                filhos.Add(new Cidade(filhoRota));
            }

            return filhos;
        }

        public static List<Cidade> AplicarMutacao(List<Cidade> populacao, int eliteSize, int taxa, int estagnacao)
        {
            Random random = new();
            int mutacoes = (populacao.Count - eliteSize) * taxa / 100;

            for (int i = eliteSize; i < populacao.Count && mutacoes > 0; i++)
            {
                if (random.NextDouble() < 0.5 || estagnacao > 5)
                {
                    var novaRota = new List<int>(populacao[i].Rota);
                    int i1 = random.Next(novaRota.Count);
                    int i2 = random.Next(novaRota.Count);
                    (novaRota[i1], novaRota[i2]) = (novaRota[i2], novaRota[i1]);

                    populacao[i] = new Cidade(novaRota);
                    mutacoes--;
                }
            }

            return populacao;
        }

        public static (List<Cidade>, int) LidarComEstagnacao(List<Cidade> populacao, int tamanhoPopulacao, int numCidades, List<int> historicoAptidao, int estagnacaoAtual)
        {
            if (historicoAptidao.Count < 2)
                return (populacao, estagnacaoAtual);

            int melhorAtual = populacao[0].Aptidao;
            int melhorAnterior = historicoAptidao[historicoAptidao.Count - 2];

            if (melhorAtual == melhorAnterior)
            {
                estagnacaoAtual++;
                if (estagnacaoAtual >= 5)
                {
                    Console.WriteLine("⚠️ Estagnação detectada! Gerando nova população parcial...");
                    var novaPop = GerarPopulacao(tamanhoPopulacao, numCidades);
                    novaPop[0] = populacao[0]; // mantém o melhor
                    return (novaPop, 0);
                }
            }
            else
            {
                estagnacaoAtual = 0;
            }

            return (populacao, estagnacaoAtual);
        }

        public static List<Cidade> ReiniciarPopulacaoPorEstagnacao(List<Cidade> populacao, int tamanhoPopulacao, List<int> estadoFinal)
        {
            Console.WriteLine("⚠️ Estagnação prolongada! Reiniciando população...");
            var novaPop = GerarPopulacao(tamanhoPopulacao, estadoFinal.Count);
            return novaPop;
        }
    }
}
