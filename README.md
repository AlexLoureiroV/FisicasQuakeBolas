# Bolas de Quake (Portal Physics)

Práctica de físicas en Unity (módulo 5.1.4): un cañón dispara bolas hacia un portal móvil; la bola que lo cruza aparece en un circuito y pasa a controlarla el jugador.

Versión de Unity: 6000.3.16f1 (URP). Escena: `Assets/Scenes/SampleScene.unity`.

El enunciado del ejercicio, con el prompt usado y la propuesta de la IA, está en [ENUNCIADO.md](ENUNCIADO.md).

## Cómo se juega

1. Apunta el cañón y mantén **Espacio** para cargar fuerza; suelta para disparar.
2. Si la bola atraviesa el **portal de origen** (se mueve de lado a lado), se teletransporta al **portal de destino** y la cámara la sigue.
3. Conduce la bola por el circuito: suelo de aceleración, barro que frena, portal de impulso y paredes que rebotan.
4. Suma puntos acertando en la **diana** (más puntos cuanto más al centro) o lanzando un **destello** cerca de los bloques del final: un halo que crece desde la bola y destruye lo que alcanza.
5. Al llegar a 100 puntos, victoria.

## Controles

| Tecla | Con el cañón | Con la bola |
|-------|--------------|-------------|
| A / D o ← / → | Girar el cañón | Mover la bola a los lados |
| W / S o ↑ / ↓ | Subir / bajar el ángulo | — |
| Espacio | Mantener para cargar, soltar para disparar | Saltar |
| ↓ (bola casi parada) | — | Mantener para cargar el turbo, soltar para lanzarlo (3 por bola) |
| P | — | Destello: halo que destruye los bloques en su radio (3 por bola) |
| R | Partida nueva (puntuación a cero) | Partida nueva |
| H | Mostrar / ocultar la ayuda de controles | Mostrar / ocultar la ayuda de controles |

Abajo a la izquierda hay un panel de ayuda con los controles del modo actual; cada tecla se ilumina al pulsarla.

El cañón se ilumina en verde cuando el raycast de puntería toca el portal de origen.

## Qué se practica

| Concepto | Dónde |
|----------|-------|
| `Rigidbody.AddForce` / `AddTorque` | Disparo (`SceneController`), salto y barro (`Ball`), suelo de aceleración (`SpeedBoostFloor`) |
| `AddExplosionForce` + `Physics.OverlapSphere` | Destello con halo expansivo (`Ball`) |
| `linearVelocity` | Control lateral de la bola, teletransporte, `BoostPortal` |
| Triggers (`OnTriggerEnter/Stay/Exit`) | Portales, barro, suelo de aceleración, diana |
| `Physics.Raycast` | Ayuda de puntería del cañón (`SceneController.UpdateAim`) |
| `Quaternion.Euler` | Rotación del cañón |
| Spawn point con `Transform` | `shootPoint` del cañón, salida del portal de destino |
| Variable estática | Puntuación total (`ScoreManager`) |
| Physic Materials | `BouncyWall` (rebote) |

## Scripts

Todos en `Assets/Scripts/`, con el esquema de regiones del curso (Properties, Fields, Unity Callbacks, Public Methods, Private Methods).

| Script | Función |
|--------|---------|
| `SceneController` | Cañón (rotación, carga, disparo, raycast de puntería), movimiento del portal y teletransporte |
| `Ball` | Bola: control manual, salto, turbo, destellos, barro y puntuación en la diana |
| `Portal` | Portal de origen / destino |
| `ControlsHUD` | Panel de ayuda con los controles; se crea solo al empezar la partida |
| `BallCamera` | Cámara que sigue a la bola y vuelve al cañón cuando la bola desaparece |
| `ScoreManager` | Puntuación total estática y textos de la UI |
| `ScoreTarget` | Zona de la diana con su valor en puntos |
| `DestructibleBlock` | Bloque que da puntos al ser alcanzado por el halo del destello |
| `BoostPortal` | Multiplica la velocidad de la bola al atravesarlo |
| `SpeedBoostFloor` | Suelo que acelera la bola |

## Posibles ampliaciones

- Sonido de disparo, teletransporte y explosión.
- Munición limitada o contrarreloj.
- Obstáculos móviles en el circuito.
- Apuntar el cañón con el ratón mediante raycast desde la cámara.
