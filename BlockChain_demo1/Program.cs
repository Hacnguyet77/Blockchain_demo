// See https://aka.ms/new-console-template for more information
using BlockChain_demo1;


var difficulty = new byte[] { 0x00, 0x00 };  // Thiết lập mức độ khó để đào một khối
var genesisData = System.Text.Encoding.UTF8.GetBytes("Genesis Block");

var genesisBlock = new Block(genesisData);
var blockchain = new BlockChain(difficulty, genesisBlock);

Console.WriteLine("Genesis block has been added!");
Console.WriteLine(genesisBlock);

for (int i = 0; i < 5; i++)
{
    var data = System.Text.Encoding.UTF8.GetBytes($"Block {i + 1}");
    var block = new Block(data);

    blockchain.Add(block);

    Console.WriteLine($"\nBlock {i + 1} has been added!");
    Console.WriteLine(block);


    if (!blockchain.IsValid())
    {
        Console.WriteLine("Blockchain is not valid!");
        return;
    }
}

Console.WriteLine("\nBlockchain is valid!");
