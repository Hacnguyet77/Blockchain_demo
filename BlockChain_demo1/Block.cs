using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockChain_demo1;

public interface IBlock
{
    byte[] Data { get; }
    byte[] Hash { get; }
    int Nonce { get; set; }
    byte[] PrevHash { get; set; }
    DateTime TimeStamp { get; set; }

    byte[] GenerateHash();
    void RecalculateHash();
}

public class Block : IBlock
{
    public Block(byte[] data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Nonce = 0;
        PrevHash = new byte[] { 0x00 };
        TimeStamp = DateTime.Now;
        Hash = GenerateHash();
    }

    public byte[] Data { get; private set; }
    public byte[] Hash { get; private set; }
    public int Nonce { get; set; }
    public byte[] PrevHash { get; set; }
    public DateTime TimeStamp { get; set; }

    public byte[] GenerateHash()
    {
        using (SHA256 sha = SHA256.Create())  // Change to SHA256
        using (MemoryStream st = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(st))
        {
            bw.Write(Data);
            bw.Write(Nonce);
            bw.Write(PrevHash);
            bw.Write(TimeStamp.ToString());
            return sha.ComputeHash(st.ToArray());
        }
    }

    public void RecalculateHash()
    {
        Hash = GenerateHash();
    }

    public override string ToString()
    {
        return $"{BitConverter.ToString(Hash).Replace("-", "")}:\n {BitConverter.ToString(PrevHash).Replace("-", "")}\n{Nonce} {TimeStamp}";
    }
}

public static class BlockExtension
{
    public static void Mine(this IBlock block, byte[] difficulty)
    {
        if (difficulty == null) throw new ArgumentNullException(nameof(difficulty));
        byte[] hash = new byte[0];
        while (!hash.Take(difficulty.Length).SequenceEqual(difficulty))
        {
            block.Nonce++;
            hash = block.GenerateHash();
        }
        block.RecalculateHash();
    }

    public static bool IsValid(this IBlock block)
    {
        var bk = block.GenerateHash();
        return block.Hash.SequenceEqual(bk);
    }

    public static bool IsPrevBlock(this IBlock block, IBlock prevBlock)
    {
        if (prevBlock == null) throw new ArgumentNullException(nameof(prevBlock));
        return prevBlock.IsValid() && block.PrevHash.SequenceEqual(prevBlock.Hash);
    }

    public static bool IsValid(this IEnumerable<IBlock> items)
    {
        var enums = items.ToList();
        return enums.Zip(enums.Skip(1), Tuple.Create).All(block => block.Item2.IsValid() && block.Item2.IsPrevBlock(block.Item1));
    }
}

public class BlockChain : IEnumerable<IBlock>
{
    private List<IBlock> _items = new List<IBlock>();
    private byte[] _difficulty;

    public BlockChain(byte[] difficulty, IBlock genesis)
    {
        _difficulty = difficulty;
        genesis.Mine(_difficulty);
        _items.Add(genesis);
    }

    public byte[] Difficulty { get => _difficulty; set => AdjustDifficulty(value); }

    private void AdjustDifficulty(byte[] newDifficulty)
    {
        _difficulty = newDifficulty;
    }

    public void Add(IBlock item)
    {
        if (_items.LastOrDefault() != null)
        {
            item.PrevHash = _items.LastOrDefault().Hash;
        }
        item.Mine(_difficulty);
        _items.Add(item);
    }

    public List<IBlock> Items => _items;

    public int Count => _items.Count;

    public IBlock this[int index]
    {
        get { return _items[index]; }
        set { _items[index] = value; }
    }

    public IEnumerator<IBlock> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return _items.GetEnumerator();
    }
}

