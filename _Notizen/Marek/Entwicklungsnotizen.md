# Entwicklungsnotizen

## Unity

### Vector3.Lerp vs. Vector3.MoveTowards

`Lerp` ermöglicht es die zurückgelegte Distanz mit einem prozentualem Wert anzugeben.

`MoveTowards` nutzt Schritte die man sehr gut mit einem Wert für die Geschwindigkeit anpassen kann.

### Transform.Parent

Beim Versuch den Ball vom Angreifer aufzuheben und dann an den Zielort zu bringen, kam mir die Idee den `transform.parent`zu nutzen. Problem war aber, dass der Ball seine Geschwindkeit beibehalten hat und von der Gravitation erfasst wurde.

Das Ergebnis war, dass der Vogel zwar den Ball aufgehoben hat und mit sich schleppte, aber der Ball am Ursprungsort liegen blieb und nicht wirklich vom Angreifer "hoch gehoben" wurde.

Zunächst hab ich dann probiert, dass Problem mit `useGravity = false` zu lösen aber der Ball wurde trotzdem nicht bewegt. 

Die eigentliche Lösung für das Problem war es den Ball `isKinematic = true` zu setzen. Dadurch erhielt man den gewünschten Effekt.

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