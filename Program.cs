using System;
using System.Collections.Generic;
using System.Linq;

namespace Trabalho_ia_0904
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int numCidades = 9;
            int tamanhoPopulacao = 50;

            Console.Write("Quantidade de gerações: ");
            int quantidadeGeracoes = int.Parse(Console.ReadLine());

            int taxaSelecao = 30;
            int taxaReproducao = 70;

            List<int> estadoFinal = Enumerable.Range(1, numCidades).ToList();
            List<Cidade> populacao = Cidade.GerarPopulacao(tamanhoPopulacao, numCidades);

            List<int> historicoAptidao = new List<int>();
            int estagnacaoProlongada = 0;

            populacao = populacao.OrderBy(c => c.Aptidao).ToList();
            Cidade.ExibirPopulacao(populacao, 0);
            historicoAptidao.Add(populacao[0].Aptidao);

            for (int i = 1; i <= quantidadeGeracoes; i++)
            {
                List<Cidade> novaPopulacao = new List<Cidade>();

                int eliteSize = Math.Max(1, tamanhoPopulacao / 10);
                novaPopulacao.AddRange(populacao.Take(eliteSize));

                List<Cidade> selecionados = Cidade.SelecionarPorTorneio(
                    populacao,
                    (tamanhoPopulacao * taxaSelecao / 100) - eliteSize
                );
                novaPopulacao.AddRange(selecionados);

                List<Cidade> filhos = Cidade.Reproduzir(
                    populacao,
                    tamanhoPopulacao - novaPopulacao.Count,
                    numCidades
                );
                novaPopulacao.AddRange(filhos);

                populacao = Cidade.AplicarMutacao(
                    novaPopulacao,
                    eliteSize,
                    10,
                    estagnacaoProlongada
                );

                populacao = populacao.OrderBy(c => c.Aptidao).ToList();
                int melhorAtual = populacao[0].Aptidao;
                historicoAptidao.Add(melhorAtual);

                var result = Cidade.LidarComEstagnacao(
                    populacao,
                    tamanhoPopulacao,
                    numCidades,
                    historicoAptidao,
                    estagnacaoProlongada
                );
                populacao = result.Item1;
                estagnacaoProlongada = result.Item2;

                Cidade.ExibirPopulacao(populacao, i);

                if (populacao[0].Aptidao == 0)
                {
                    Console.WriteLine("\nSolução ótima encontrada!");
                    Console.WriteLine("Estado final descoberto:");
                    Console.WriteLine(populacao[0]);
                    break;
                }

                if (estagnacaoProlongada >= 10)
                {
                    populacao = Cidade.ReiniciarPopulacaoPorEstagnacao(
                        populacao,
                        tamanhoPopulacao,
                        estadoFinal
                    );
                    estagnacaoProlongada = 0;
                }
            }
        }
    }
}
