namespace SolarMax.Integrators;

internal interface IIntegrator
{
    void Init(Physics Physics);
    void MoveOrbiters(double dt);
}
