namespace SolarMax;

internal sealed class EphemerisItem
{
    public string Name { get; private set; }
    public Orbiter Orbiter { get; private set; }
    public Vector Position { get; private set; }
    public Vector Velocity { get; private set; }

    public EphemerisItem(string Name, Vector Position, Vector Velocity)
    {
        this.Name = Name;
        this.Position = Position;
        this.Velocity = Velocity;
        this.Orbiter = null;
    }
    public EphemerisItem(Orbiter Orbiter)
    {
        this.Name = Orbiter.Name;
        this.Position = Orbiter.Position / 1000.0;
        this.Velocity = Orbiter.Velocity / 1000.0;
        this.Orbiter = Orbiter;
    }
    public void Link(Orbiter Orbiter)
    {
        this.Orbiter = Orbiter;
    }
    public bool IsLinked => this.Orbiter != null;
    public override string ToString()
    {
        if (this.Orbiter != null)
            return string.Format("{0} Linked", this.Orbiter.Name);
        else
            return this.Name;
    }
}
