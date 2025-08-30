using System;
using System.Collections.Generic;
using System.Threading;

public class Tabuleiro
{
    public int Linhas { get; }
    public int Colunas { get; }
    public int[,] OrdemPassos { get; }

    public Tabuleiro(int linhas, int colunas)
    {
        Linhas = linhas;
        Colunas = colunas;
        OrdemPassos = new int[linhas, colunas];
    }

    // Exibe o tabuleiro mostrando todos os passos e destaca o cavalo na posição atual
    public void Exibir(int cavaloLinha, int cavaloColuna)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("   ");
        for (int j = 0; j < Colunas; j++)
            Console.Write($" {j,2} ");
        Console.WriteLine();

        Console.Write("   ");
        for (int j = 0; j < Colunas; j++)
            Console.Write("+---");
        Console.WriteLine("+");

        for (int i = 0; i < Linhas; i++)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" {i,2}|");
            for (int j = 0; j < Colunas; j++)
            {
                if (i == cavaloLinha && j == cavaloColuna)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(" C ");
                }
                else if (OrdemPassos[i, j] > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"{OrdemPassos[i, j],2} ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(" . ");
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("|");
            }
            Console.WriteLine();

            Console.Write("   ");
            for (int j = 0; j < Colunas; j++)
                Console.Write("+---");
            Console.WriteLine("+");
        }
        Console.ResetColor();
    }
}

public static class MovimentoCavalo
{
    // Movimentos possíveis do cavalo
    public static readonly int[] DeltaLinha = { 2, 1, -1, -2, -2, -1, 1, 2 };
    public static readonly int[] DeltaColuna = { 1, 2, 2, 1, -1, -2, -2, -1 };
}

internal class Program
{
    private const int TamanhoTabuleiro = 8;

    private static void Main()
    {
        Console.WriteLine("Bem-vindo ao Passeio do Cavalo!");
        Console.Write($"Digite o número da linha inicial (0 a {TamanhoTabuleiro - 1}): ");
        int linhaInicial = Convert.ToInt32(Console.ReadLine());
        Console.Write($"Digite o número da coluna inicial (0 a {TamanhoTabuleiro - 1}): ");
        int colunaInicial = Convert.ToInt32(Console.ReadLine());

        Tabuleiro tabuleiro = new Tabuleiro(TamanhoTabuleiro, TamanhoTabuleiro);
        List<(int, int)> caminho = EncontrarCaminho(tabuleiro, linhaInicial, colunaInicial);

        if (caminho != null)
        {
            AnimarCaminho(tabuleiro, caminho);
            Console.WriteLine("Passeio completo!");
        }
        else
        {
            Console.WriteLine("Não foi possível encontrar um caminho completo.");
        }
    }

    // Anima o passeio do cavalo mostrando o tabuleiro passo a passo
    private static void AnimarCaminho(Tabuleiro tabuleiro, List<(int, int)> caminho)
    {
        for (int passo = 0; passo < caminho.Count; passo++)
        {
            var (linha, coluna) = caminho[passo];
            tabuleiro.OrdemPassos[linha, coluna] = passo + 1;

            Console.Clear();
            tabuleiro.Exibir(linha, coluna);
            Console.WriteLine($"Passo: {passo + 1}");
            Thread.Sleep(500);
            //Console.ReadKey();
        }
    }

    // Encontra o passeio do cavalo usando busca heurística (Warnsdorff)
    private static List<(int, int)> EncontrarCaminho(Tabuleiro tabuleiro, int linhaInicial, int colunaInicial)
    {
        int[,] visitado = new int[tabuleiro.Linhas, tabuleiro.Colunas];
        List<(int, int)> caminho = new();
        if (Buscar(tabuleiro, linhaInicial, colunaInicial, 1, visitado, caminho))
            return caminho;
        return null;
    }

    // Busca recursiva com heurística de Warnsdorff
    private static bool Buscar(Tabuleiro tabuleiro, int linha, int coluna, int passo, int[,] visitado, List<(int, int)> caminho)
    {
        visitado[linha, coluna] = passo;
        caminho.Add((linha, coluna));

        if (passo == tabuleiro.Linhas * tabuleiro.Colunas)
            return true;

        // Gera movimentos possíveis e ordena pelo número de opções futuras (Warnsdorff)
        var movimentos = new List<(int linha, int coluna, int grau)>();
        for (int i = 0; i < 8; i++)
        {
            int novaLinha = linha + MovimentoCavalo.DeltaLinha[i];
            int novaColuna = coluna + MovimentoCavalo.DeltaColuna[i];
            if (PosicaoValida(tabuleiro, novaLinha, novaColuna, visitado))
            {
                int grau = ContarMovimentos(tabuleiro, novaLinha, novaColuna, visitado);
                movimentos.Add((novaLinha, novaColuna, grau));
            }
        }
        movimentos.Sort((a, b) => a.grau.CompareTo(b.grau));

        foreach (var (novaLinha, novaColuna, _) in movimentos)
        {
            if (Buscar(tabuleiro, novaLinha, novaColuna, passo + 1, visitado, caminho))
                return true;
        }

        visitado[linha, coluna] = 0;
        caminho.RemoveAt(caminho.Count - 1);
        return false;
    }

    // Verifica se a posição é válida e não visitada
    private static bool PosicaoValida(Tabuleiro tabuleiro, int linha, int coluna, int[,] visitado)
    {
        return linha >= 0 && linha < tabuleiro.Linhas &&
               coluna >= 0 && coluna < tabuleiro.Colunas &&
               visitado[linha, coluna] == 0;
    }

    // Conta quantos movimentos futuros são possíveis a partir de uma posição
    private static int ContarMovimentos(Tabuleiro tabuleiro, int linha, int coluna, int[,] visitado)
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            int novaLinha = linha + MovimentoCavalo.DeltaLinha[i];
            int novaColuna = coluna + MovimentoCavalo.DeltaColuna[i];
            if (PosicaoValida(tabuleiro, novaLinha, novaColuna, visitado))
                count++;
        }
        return count;
    }
}