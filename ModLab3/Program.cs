using System;
using System.Collections.Generic;
using System.Text;

public class Fish
{
    public int Age { get; set; }
    public int MaxAge { get; set; }
    public int ReproductionAge { get; set; }
    public bool IsPredator { get; set; }
    public bool IsAlive { get; set; } = true;
    public (int, int) Position { get; set; }

    public Fish(int age, int maxAge, int reproductionAge, bool isPredator, (int, int) position)
    {
        Age = age;
        MaxAge = maxAge;
        ReproductionAge = reproductionAge;
        IsPredator = isPredator;
        Position = position;
    }

    public virtual void Move((int, int) newPosition)
    {
        Position = newPosition;
    }

    public virtual bool CanReproduce()
    {
        return Age >= ReproductionAge;
    }

    public virtual void AgeOneTick()
    {
        Age++;
        if (Age > MaxAge)
        {
            IsAlive = false;
        }
    }
}

public class Pike : Fish
{
    public int HungerTime { get; set; }
    public int MaxHungerTime { get; set; }

    public Pike(int age, int maxAge, int reproductionAge, int maxHungerTime, (int, int) position)
        : base(age, maxAge, reproductionAge, true, position)
    {
        HungerTime = 0;
        MaxHungerTime = maxHungerTime;
    }

    public bool IsHungry()
    {
        return HungerTime >= MaxHungerTime;
    }

    public void Eat()
    {
        HungerTime = 0;
    }

    public override void AgeOneTick()
    {
        base.AgeOneTick();
        HungerTime++;
        if (IsHungry())
        {
            IsAlive = false;
        }
    }
}

public class Carp : Fish
{
    public Carp(int age, int maxAge, int reproductionAge, (int, int) position)
        : base(age, maxAge, reproductionAge, false, position)
    {
    }
}

public class Pond
{
    private readonly int _width;
    private readonly int _height;
    private readonly Fish[,] _grid;

    public Pond(int width, int height)
    {
        _width = width;
        _height = height;
        _grid = new Fish[width, height];
    }

    public Fish GetFishAt(int x, int y)
    {
        return _grid[x, y];
    }

    public void PlaceFish(Fish fish, int x, int y)
    {
        _grid[x, y] = fish;
    }

    public void MoveFish(Fish fish, int newX, int newY)
    {
        _grid[fish.Position.Item1, fish.Position.Item2] = null;
        _grid[newX, newY] = fish;
        fish.Move((newX, newY));
    }

    public void RemoveFish(int x, int y)
    {
        _grid[x, y] = null;
    }

    public (int, int) GetRandomFreePosition()
    {
        Random random = new Random();
        int x, y;
        do
        {
            x = random.Next(_width);
            y = random.Next(_height);
        } while (_grid[x, y] != null);

        return (x, y);
    }

    public (int, int)[] GetAdjacentPositions(int x, int y)
    {
        List<(int, int)> positions = new List<(int, int)>();

        if (x > 0) positions.Add((x - 1, y));
        if (x < _width - 1) positions.Add((x + 1, y));
        if (y > 0) positions.Add((x, y - 1));
        if (y < _height - 1) positions.Add((x, y + 1));

        return positions.ToArray();
    }

    public void Display()
    {
        StringBuilder sb = new StringBuilder();
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Fish fish = _grid[x, y];
                if (fish == null)
                {
                    sb.Append(". ");
                }
                else if (fish.IsPredator)
                {
                    sb.Append("P ");
                }
                else
                {
                    sb.Append("C ");
                }
            }
            sb.AppendLine();
        }
        Console.WriteLine(sb.ToString());
    }
}

public class PredatorPreySimulation
{
    private readonly Pond _pond;
    private readonly List<Pike> _pikes;
    private readonly List<Carp> _carps;

    public PredatorPreySimulation(int width, int height, int numPikes, int numCarps)
    {
        _pond = new Pond(width, height);
        _pikes = new List<Pike>();
        _carps = new List<Carp>();

        InitializeFish(numPikes, numCarps);
    }

    private void InitializeFish(int numPikes, int numCarps)
    {
        Random random = new Random();

        for (int i = 0; i < numPikes; i++)
        {
            var position = _pond.GetRandomFreePosition();
            Pike pike = new Pike(0, 20, 5, 3, position);
            _pikes.Add(pike);
            _pond.PlaceFish(pike, position.Item1, position.Item2);
        }

        for (int i = 0; i < numCarps; i++)
        {
            var position = _pond.GetRandomFreePosition();
            Carp carp = new Carp(0, 10, 3, position);
            _carps.Add(carp);
            _pond.PlaceFish(carp, position.Item1, position.Item2);
        }
    }

    public void SimulateTick()
    {
        foreach (var carp in _carps)
        {
            if (carp.IsAlive)
            {
                var newPosition = _pond.GetRandomFreePosition();
                _pond.MoveFish(carp, newPosition.Item1, newPosition.Item2);
                carp.AgeOneTick();
            }
        }

        foreach (var pike in _pikes)
        {
            if (pike.IsAlive)
            {
                var adjacentPositions = _pond.GetAdjacentPositions(pike.Position.Item1, pike.Position.Item2);
                bool ateCarp = false;

                foreach (var pos in adjacentPositions)
                {
                    Fish adjacentFish = _pond.GetFishAt(pos.Item1, pos.Item2);
                    if (adjacentFish is Carp && adjacentFish.IsAlive)
                    {// Pike eats the carp
                        _pond.RemoveFish(pos.Item1, pos.Item2);
                        pike.Eat();
                        ateCarp = true;
                        break;
                    }
                }

                if (!ateCarp)
                {
                    // If no carp was eaten, move the pike to a random free cell
                    var newPosition = _pond.GetRandomFreePosition();
                    _pond.MoveFish(pike, newPosition.Item1, newPosition.Item2);
                }

                pike.AgeOneTick();
            }
        }

        // Remove dead fish
        _pikes.RemoveAll(p => !p.IsAlive);
        _carps.RemoveAll(c => !c.IsAlive);
    }

    public void DisplayPond()
    {
        _pond.Display();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        int width = 10;
        int height = 10;
        int numPikes = 5;
        int numCarps = 20;

        PredatorPreySimulation simulation = new PredatorPreySimulation(width, height, numPikes, numCarps);

        for (int tick = 0; tick < 20; tick++) // Simulate 20 ticks
        {
            Console.Clear();
            Console.WriteLine($"Tick {tick + 1}:");
            simulation.DisplayPond();
            simulation.SimulateTick();
            System.Threading.Thread.Sleep(500); // Pause for half a second between ticks
        }

        Console.WriteLine("Simulation ended.");
    }
}
