namespace Arbiter.CommandQuery.Tests.Models;

public class Fruit
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int Rank { get; set; }

    public Dictionary<string, string>? Tags { get; set; }

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(Rank)}: {Rank}";
    }

    public static List<Fruit> Data()
    {
        return
        [
            new Fruit
            {
                Id = new("3a1ec4ee-239c-41e5-b934-fbe4ce8113df"),
                Name = "Pear",
                Rank = 1,
                Tags = new() { ["color"] = "green", ["season"] = "fall" }
            },
            new Fruit
            {
                Id = new("1109c1a7-65e3-4006-9611-c359f3d1f086"),
                Name = "Pineapple",
                Rank = 4,
                Tags = new() { ["color"] = "yellow", ["season"] = "summer" }
            },
            new Fruit
            {
                Id = new("0d830fec-e023-438f-bcf6-1b3cba1245e6"),
                Name = "Peach",
                Rank = 2,
                Tags = new() { ["color"] = "orange", ["season"] = "summer" }
            },
            new Fruit
            {
                Id = new("bb7aa825-bdbb-4cda-9c12-131c13e02bea"),
                Name = "Apple",
                Rank = 3,
                Tags = new() { ["color"] = "red", ["season"] = "fall" }
            },
            new Fruit
            {
                Id = new("5fef330b-6f30-461d-95e2-d526b4669e76"),
                Name = "Grape",
                Rank = 5,
                Tags = new() { ["color"] = "purple", ["season"] = "fall" }
            },
            new Fruit
            {
                Id = new("50611bc9-dac8-4552-b556-f252b1cff0d3"),
                Name = "Orange",
                Rank = 6,
                Tags = new() { ["color"] = "orange", ["season"] = "winter" }
            },
            new Fruit
            {
                Id = new("5f467286-e321-44df-8a27-2c52a5ceed64"),
                Name = "Strawberry",
                Rank = 7,
                Tags = new() { ["color"] = "red", ["season"] = "spring" }
            },
            new Fruit
            {
                Id = new("ef628e60-500a-4663-8b06-548b0f5857de"),
                Name = "Blueberry",
                Rank = 7,
                Tags = new() { ["color"] = "blue", ["season"] = "summer" }
            },
            new Fruit
            {
                Id = new("dc0a7dc7-40de-430d-a8fc-e17370c2a773"),
                Name = "Banana",
                Rank = 8,
                Tags = new() { ["color"] = "yellow", ["season"] = "all" }
            },
            new Fruit
            {
                Id = new("98620233-75c5-4213-966c-9c6bfcf9e8d5"),
                Name = "Raspberry",
                Rank = 7,
                Tags = new() { ["color"] = "red", ["season"] = "summer" }
            }
        ];
    }
}
