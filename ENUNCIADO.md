# Enunciado del ejercicio (generado con IA)

## Prompt

> Estamos en un curso de desarrollo de videojuegos con Unity, acabamos de ver el tema de físicas, con el AddForce, trigger y collision detection, velocity y raycast. Hemos hecho un sencillo control de personaje con físicas. Pero nos gustaría hacer dos ejercicios más, sencillos y originales. ¿Qué nos propones?

## Respuesta de la IA

¡Buen momento para practicar! Os propongo dos ejercicios cortos que usan todo lo que habéis visto. Se pueden hacer por separado, pero están pensados para encadenarse: el primero lanza la bola y el segundo la lleva hasta el final del nivel.

---

### Ejercicio 1: Bolas de Quake (cañón y portales)

Un cañón dispara bolas hacia un portal que se mueve de lado a lado. Si la bola lo atraviesa, aparece por un segundo portal en otra parte del escenario.

**Qué hay que construir**

1. Un escenario sencillo con suelo y paredes (cubos con collider).
2. Un cañón hecho con un cilindro, con dos pivotes: uno para girar en horizontal y otro para subir y bajar el ángulo. Usa `Quaternion.Euler` para aplicar los ángulos.
3. Un `SceneController` con los valores del disparo expuestos en el inspector (ángulo, fuerza mínima y máxima) usando `[Range]`.
4. Disparo con carga: mientras se mantiene Espacio la fuerza sube; al soltar se instancia la bola en un **spawn point** (un `Transform`, no un `Vector3` fijo) y se le aplica `AddForce` con `ForceMode.Impulse` en la dirección del cañón.
5. Dos portales con collider en modo **trigger**:
   - El portal de origen se mueve de lado a lado con `Mathf.Sin`.
   - Al entrar la bola (`OnTriggerEnter`), se coloca en el portal de destino y se reorienta su `velocity` hacia el eje `up` del portal, conservando la rapidez.
6. Una **diana** con varias zonas concéntricas que den distinta puntuación (por ejemplo 10, 25 y 50). Cada zona tiene su script con una variable de puntos.
7. Una puntuación total guardada en una **variable estática** y mostrada en pantalla con TextMeshPro.

**Conceptos que practica:** `AddForce`, `velocity`, triggers, spawn points, `Quaternion.Euler`, variables estáticas.

**Retos extra**

- Usar un **raycast** desde la boca del cañón para avisar (por ejemplo, cambiando su color) cuando apunta al portal.
- Hacer que el cañón brille más cuanto más carga el disparo.
- Usar Mesh Colliders (convex + trigger) para que la diana detecte el impacto con su forma real.

---

### Ejercicio 2: Circuito de la bola

Cuando la bola sale del portal de destino, el jugador pasa a controlarla y tiene que llevarla por un pequeño circuito con zonas que cambian su física.

**Qué hay que construir**

1. Control de la bola con físicas: movimiento lateral modificando su `velocity` y salto con `AddForce` (con un tiempo de espera para no volar).
2. Una cámara que siga a la bola de forma suave (`Vector3.Lerp` en `LateUpdate`).
3. Zonas del circuito, cada una con su trigger o material:
   - **Barro:** frena la bola aplicando una fuerza contraria a su velocidad mientras está dentro (`OnTriggerEnter` / `OnTriggerExit`).
   - **Suelo de aceleración:** empuja la bola mientras está encima (`OnTriggerStay`), con una velocidad máxima.
   - **Portal de impulso:** multiplica la velocidad de la bola al atravesarlo.
   - **Pared de rebote:** usa un Physic Material con rebote máximo.
4. Bloques destructibles al final del circuito que den puntos al romperse.
5. Una tecla que lance un **destello**: una onda que crece desde la bola, detecta los objetos cercanos con `Physics.OverlapSphere`, rompe los bloques y empuja los demás con `AddExplosionForce`.

**Conceptos que practica:** `velocity`, `AddForce`, `AddExplosionForce`, `OverlapSphere`, triggers (`Enter`, `Stay`, `Exit`), Physic Materials.

**Retos extra**

- Limitar los destellos (por ejemplo, 3 por bola) y mostrarlos en pantalla.
- Añadir un **turbo** que se carga manteniendo una tecla cuando la bola va lenta y la impulsa solo hacia delante; la bola puede cambiar de color mientras carga.
- Mostrar en pantalla un panel con los controles para que cualquiera sepa jugar al abrirlo.
- Un objetivo de puntuación que muestre un mensaje de victoria.

---

### Consejos

- Dad nombres claros a los objetos de la escena y agrupadlos (cañón, portales, diana, circuito).
- Comprobad que el nombre de cada script coincide con el de su clase.
- Si algo da `NullReferenceException`, revisad que todas las referencias del inspector estén asignadas.
- Subid el proyecto a GitHub con un README que explique los controles.
