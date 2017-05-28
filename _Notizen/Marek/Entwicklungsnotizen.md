# Entwicklungsnotizen

## Unity

### Windturbine, Gizmos und Beschleunigung

- Wind ist mit einem `BoxCollider` realisiert
- Da die Mesh einer Box die Sicht im Editor stört wurde die Mesh abgeschaltet
- Problem war jetzt die Positionierung der Windrichtung, da es keine visuelle Unterstützung gab
- Lösung dafür waren dann Gizmos:

```cs
private void OnDrawGizmos()
{
    BoxCollider area = GetComponent<BoxCollider>();
    Gizmos.color = Color.green;
    Gizmos.DrawWireCube(area.center, area.size);
}
```

Erste Idee einer Windturbine:
- Der Ball bewegt sich durch einen Trigger
- Bekommt vom Trigger eine neue Beschleunigung
- Turbine war nach rechts gerichtet daher ball mit  
``` ball.AddForce(Vector3.right * windForce, ForceMode.Acceleration) ```  
beschleunigt
- Hat wunderbar funktioniert nur der Ball wurde immer nach rechts beschleunigt
ha

```cs
Rigidbody ball = other.attachedRigidbody;

Vector3 locVel = transform.InverseTransformDirect(ball.velocity);
locVel.z = windForce;
ball.velocity = transform.TransformDirection(locVel);
``` 


## Whatever 1
## Whatever 2