using System.Diagnostics;

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

    public void Exibir(int cavaloLinha, int cavaloColuna)
    {
        Console.Write("   ");
        for (int j = 0; j < Colunas; j++)
            Console.Write($"{j,2} ");
        Console.WriteLine();

        for (int i = 0; i < Linhas; i++)
        {
            Console.Write($"{i,2} ");
            for (int j = 0; j < Colunas; j++)
            {
                if (i == cavaloLinha && j == cavaloColuna)
                    Console.Write(" C ");
                else if (OrdemPassos[i, j] > 0)
                    Console.Write($"{OrdemPassos[i, j],2} ");
                else
                    Console.Write(" . ");
            }
            Console.WriteLine();
        }
    }
}

public static class MovimentoCavalo
{
    public static readonly int[] DeltaLinha = {2, 1, -1, -2, -2, -1, 1, 2};
    public static readonly int[] DeltaColuna = {1, 2, 2, 1, -1, -2, -2, -1};
}

internal class Program
{
    private static int _tamanhoTabuleiro = 8;

    private static void Main()
    {
        Console.WriteLine("Bem-vindo ao Passeio do Cavalo!");
        Console.Write("Digite o tamanho do tabuleiro: ");
        _tamanhoTabuleiro = Convert.ToInt32(Console.ReadLine());
        Console.Write($"Digite o número da linha inicial (0 a {_tamanhoTabuleiro - 1}): ");
        int linhaInicial = Convert.ToInt32(Console.ReadLine());
        Console.Write($"Digite o número da coluna inicial (0 a {_tamanhoTabuleiro - 1}): ");
        int colunaInicial = Convert.ToInt32(Console.ReadLine());

        Tabuleiro tabuleiro = new Tabuleiro(_tamanhoTabuleiro, _tamanhoTabuleiro);

        bool modoCalculo = _tamanhoTabuleiro > 10;

        var cronometro = Stopwatch.StartNew();
        List<(int, int)>? caminho = EncontrarCaminho(tabuleiro, linhaInicial, colunaInicial);

        if (caminho != null)
        {
            if (modoCalculo)
            {
                Console.WriteLine($"[MODO CÁLCULO] Passeio completo em {caminho.Count} passos.");
                Console.WriteLine($"Tempo de execução: {cronometro.ElapsedMilliseconds} ms");
                Console.Write("Caminho: ");
                for (int i = 0; i < caminho.Count; i++)
                {
                    var (linha, coluna) = caminho[i];
                    Console.Write($"({linha},{coluna})");
                    if (i < caminho.Count - 1)
                        Console.Write(" -> ");
                }
                Console.WriteLine();
            }
            else
            {
                AnimarCaminho(tabuleiro, caminho);
                Console.WriteLine("Passeio completo!");
            }
            Console.ReadKey();
        }
        else
        {
            Console.WriteLine("Não foi possível encontrar um caminho completo.");
        }
    }

    private static void AnimarCaminho(Tabuleiro tabuleiro, List<(int, int)> caminho, bool passoAPasso = true)
    {
        for (int passo = 0; passo < caminho.Count; passo++)
        {
            var (linha, coluna) = caminho[passo];
            tabuleiro.OrdemPassos[linha, coluna] = passo + 1;

            if (passoAPasso)
            {
                Console.Clear();
                tabuleiro.Exibir(linha, coluna);
                Console.WriteLine($"Passo: {passo + 1}");
                Thread.Sleep(200); // animacao
                //Console.ReadKey();
            }
        }
        Console.Clear();
        tabuleiro.Exibir(-1, -1);
    }

    private static List<(int, int)>? EncontrarCaminho(Tabuleiro tabuleiro, int linhaInicial, int colunaInicial)
    {
        int[,] visitado = new int[tabuleiro.Linhas, tabuleiro.Colunas];
        List<(int, int)> caminho = new();
        if (Buscar(tabuleiro, linhaInicial, colunaInicial, 1, visitado, caminho))
            return caminho;
        return null;
    }

    // Busca recursiva com Warnsdorff
    private static bool Buscar(Tabuleiro tabuleiro, int linha, int coluna, int passo, int[,] visitado, List<(int, int)> caminho)
    {
        visitado[linha, coluna] = passo;
        caminho.Add((linha, coluna));

        if (passo == tabuleiro.Linhas * tabuleiro.Colunas)
            return true;

        //gera movimentos possíveis e ordena pelo grau
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

    private static bool PosicaoValida(Tabuleiro tabuleiro, int linha, int coluna, int[,] visitado)
    {
        return linha >= 0 && linha < tabuleiro.Linhas &&
               coluna >= 0 && coluna < tabuleiro.Colunas &&
               visitado[linha, coluna] == 0;
    }

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