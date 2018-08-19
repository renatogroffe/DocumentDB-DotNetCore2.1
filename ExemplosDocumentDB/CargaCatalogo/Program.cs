using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CargaCatalogo
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json");
            var configuration = builder.Build();

            Console.WriteLine("Obter configurações de acesso...");
            DocumentClient client = new DocumentClient(
                new Uri(configuration.GetSection("DBCatalogo:EndpointUri").Value),
                configuration.GetSection("DBCatalogo:PrimaryKey").Value);

            Console.WriteLine("Criar banco de dados...");
            client.CreateDatabaseAsync(
                new Database { Id = "DBCatalogo" }).Wait();

            Console.WriteLine("Criar coleção...");
            DocumentCollection collectionInfo = new DocumentCollection();
            collectionInfo.Id = "Catalogo";

            collectionInfo.IndexingPolicy =
                new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

            client.CreateDocumentCollectionAsync(
                UriFactory.CreateDatabaseUri("DBCatalogo"),
                collectionInfo,
                new RequestOptions { OfferThroughput = 400 }).Wait();

            Console.WriteLine("Incluir produtos...");
            Carga.InserirDadosProdutos(client);

            Console.WriteLine("Incluir serviços...");
            Carga.InserirDadosServicos(client);

            Console.WriteLine("Finalizado!");
            Console.ReadKey();
        }
    }
}