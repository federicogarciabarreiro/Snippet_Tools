# Snippets (Unity Systems)

## ES - Que es esta carpeta
Esta carpeta contiene muestras públicas de código arquitectura y patrones de diseño para sistemas Unity.
El objetivo es mostrar capacidad técnica con ejemplos claros, reutilizables y seguros (sin código literal de empresas).

## EN - What this folder is
This folder contains public code samples of architecture and design patterns for Unity systems.
The goal is to show technical capability with clear, reusable examples (no literal company code).

---

## ES - 01 AI System - Embeddings & Contextual Dialog
Archivo: `01_AI_System_Example.cs`

Paso a paso:
1. Define interfaz `IADialog` extensible para sistemas de diálogo.
2. Implementa vector embeddings usando `Mathf` y búsqueda por similitud (cosine distance).
3. Usa LINQ `Zip()` y `Sum()` para calcular distancia vectorial entre embeddings.
4. Cachea embeddings calculados para optimizar búsquedas repetidas.
5. Soporta búsqueda contextual filtrando respuestas por similitud vectorial.
6. Integra con `LocalizationManager` para respuestas multiidioma.

## EN - 01 AI System - Embeddings & Contextual Dialog
File: `01_AI_System_Example.cs`

Step by step:
1. Defines extensible `IADialog` interface for dialog systems.
2. Implements vector embeddings using `Mathf` and similarity search (cosine distance).
3. Uses LINQ `Zip()` and `Sum()` to calculate vector distance between embeddings.
4. Caches computed embeddings to optimize repeated searches.
5. Supports contextual search by filtering responses via vector similarity.
6. Integrates with `LocalizationManager` for multi-language responses.

---

## ES - 02 Dialog System - Branching Narrative & Localization
Archivo: `02_Dialog_System_Example.cs`

Paso a paso:
1. Define nodos de diálogo con condiciones booleanas (`isAvailable`, `onComplete`).
2. Filtra ramas disponibles usando LINQ `Where()` basado en condiciones.
3. Implementa callbacks para ejecutar acciones cuando se selecciona una rama.
4. Integra `LocalizationManager` para traducir líneas de diálogo dinámicamente.
5. Cachea nodos evaluados para optimizar búsqueda de siguiente nodo.
6. Soporta eventos UnityEvent para disparar comportamiento del juego en diálogos.

## EN - 02 Dialog System - Branching Narrative & Localization
File: `02_Dialog_System_Example.cs`

Step by step:
1. Defines dialog nodes with boolean conditions (`isAvailable`, `onComplete`).
2. Filters available branches using LINQ `Where()` based on conditions.
3. Implements callbacks to execute actions when a branch is selected.
4. Integrates `LocalizationManager` to dynamically translate dialog lines.
5. Caches evaluated nodes to optimize next-node lookup.
6. Supports UnityEvent to trigger game behavior during dialogs.

---

## ES - 03 Interaction System - Object Detection & State Management
Archivo: `03_Interaction_System_Example.cs`

Paso a paso:
1. Lanza raycast desde cámara en dirección del click del jugador.
2. Filtra objetos interactivos usando LINQ `Where()` por distancia máxima.
3. Valida que el objeto implemente interfaz `IInteractable`.
4. Invoca callback `OnInteract()` pasando contexto del jugador.
5. Actualiza estado interno del objeto interactivo (animación, sonido).
6. Cachea el último objeto interactuable para evitar raycasts repetidos.

## EN - 03 Interaction System - Object Detection & State Management
File: `03_Interaction_System_Example.cs`

Step by step:
1. Casts raycast from camera in direction of player click.
2. Filters interactive objects using LINQ `Where()` by maximum distance.
3. Validates that object implements `IInteractable` interface.
4. Invokes `OnInteract()` callback passing player context.
5. Updates interactive object's internal state (animation, sound).
6. Caches last interactable object to avoid repeated raycasts.

---

## ES - 04 Navigation System - Pathfinding & Autonomous Movement
Archivo: `04_Navigation_System_Example.cs`

Paso a paso:
1. Integra `NavMeshAgent` para calcular ruta automática hacia destino.
2. Suaviza la ruta calculada usando Catmull-Rom spline para movimiento natural.
3. Ajusta velocidad dinámicamente según tipo de terreno (curva de velocidad por área).
4. Valida waypoints usando LINQ `Where()` para ignorar puntos inaccesibles.
5. Detecta llegada a destino comprobando distancia residual con `Vector3.Distance()`.
6. Implementa corrección de ruta si NavMesh cambia dinámicamente.

## EN - 04 Navigation System - Pathfinding & Autonomous Movement
File: `04_Navigation_System_Example.cs`

Step by step:
1. Integrates `NavMeshAgent` to automatically calculate path to destination.
2. Smooths computed path using Catmull-Rom spline for natural movement.
3. Dynamically adjusts speed based on terrain type (speed curve per area).
4. Validates waypoints using LINQ `Where()` to ignore unreachable points.
5. Detects destination arrival by checking residual distance with `Vector3.Distance()`.
6. Implements path correction if NavMesh changes dynamically.

---

## ES - 05 Physics System - Ragdoll & Impact Calculations
Archivo: `05_Physics_System_Example.cs`

Paso a paso:
1. Detecta colisión y calcula magnitud del impacto usando `ContactPoint.relativeVelocity`.
2. Calcula daño basado en velocidad de impacto y multiplier de material.
3. Encuentra el hueso ragdoll más cercano usando LINQ `OrderBy()` por distancia.
4. Aplica fuerza direccional al rigidbody del ragdoll para reacción física realista.
5. Desactiva Animator y activa componente de ragdoll de forma suave (lerp).
6. Dispara evento de impacto para sonido efectos de partículas.

## EN - 05 Physics System - Ragdoll & Impact Calculations
File: `05_Physics_System_Example.cs`

Step by step:
1. Detects collision and calculates impact magnitude using `ContactPoint.relativeVelocity`.
2. Calculates damage based on impact velocity and material multiplier.
3. Finds closest ragdoll bone using LINQ `OrderBy()` by distance.
4. Applies directional force to ragdoll rigidbody for realistic physical reaction.
5. Disables Animator and activates ragdoll component smoothly (lerp).
6. Dispatches impact event for sound and particle effects.

---

## ES - 06 Animation System - Procedural & State-Driven
Archivo: `06_Animation_System_Example.cs`

Paso a paso:
1. Define máquina de estados con callback dictionary mapeando estado → método.
2. Implementa IK procedural usando `Animator.SetIKGoalPosition()` en `OnAnimatorIK`.
3. Sincroniza sonido con animación usando corrutinas `WaitForSeconds` en tiempo de frame.
4. Valida transiciones entre estados usando condiciones booleanas.
5. Cachea parámetros de Animator para evitar búsquedas repetidas.
6. Proporciona sistema de log para debug de transiciones de estado.

## EN - 06 Animation System - Procedural & State-Driven
File: `06_Animation_System_Example.cs`

Step by step:
1. Defines state machine with callback dictionary mapping state → method.
2. Implements procedural IK using `Animator.SetIKGoalPosition()` in `OnAnimatorIK`.
3. Synchronizes sound with animation using coroutines `WaitForSeconds` at frame time.
4. Validates state transitions using boolean conditions.
5. Caches Animator parameters to avoid repeated lookups.
6. Provides debug logging system for state transition tracing.

---

## ES - 07 FPS System - Ballistics & Hit Detection
Archivo: `07_FPS_System_Example.cs`

Paso a paso:
1. Implementa armas hitscan lanzando raycast desde punto de disparo.
2. Valida colisiones ignorando trigger colliders usando máscara de layer.
3. Calcula daño con falloff de distancia: `damage * (1 - distance/maxRange)`.
4. Genera decal de impacto en punto de hit con rotación normal a superficie.
5. Implementa balística para proyectiles calculando trayectoria con gravedad.
6. Aplica recoil visual mediante lerp de rotación y spread de disparo aleatorio.

## EN - 07 FPS System - Ballistics & Hit Detection
File: `07_FPS_System_Example.cs`

Step by step:
1. Implements hitscan weapons by casting raycast from firing point.
2. Validates collisions while ignoring trigger colliders using layer mask.
3. Calculates damage with distance falloff: `damage * (1 - distance/maxRange)`.
4. Generates impact decal at hit point rotated normal to surface.
5. Implements ballistics for projectiles calculating trajectory with gravity.
6. Applies visual recoil via lerp rotation and random firing spread.

---

## ES - 08 UI System - Responsive Layout & Data Binding
Archivo: `08_UI_System_Example.cs`

Paso a paso:
1. Implementa clase genérica `BindableProperty<T>` que dispara evento al cambiar valor.
2. Los elementos UI se suscriben a cambios de propiedad para actualizar automáticamente.
3. Ajusta layouts responsive verificando aspect ratio y orientación del dispositivo.
4. Implementa sistema de temas aplicando colores/fuentes usando LINQ `ForEach()`.
5. Cachea referencias a componentes Text/Image para evitar búsquedas repetidas.
6. Soporta pooling de elementos UI frecuentes (botones, listas) para optimizar GC.

## EN - 08 UI System - Responsive Layout & Data Binding
File: `08_UI_System_Example.cs`

Step by step:
1. Implements generic `BindableProperty<T>` class that fires event on value change.
2. UI elements subscribe to property changes to automatically update.
3. Adjusts responsive layouts by checking device aspect ratio and orientation.
4. Implements theme system applying colors/fonts using LINQ `ForEach()`.
5. Caches references to Text/Image components to avoid repeated lookups.
6. Supports pooling of frequent UI elements (buttons, lists) to optimize GC.

---

## ES - 09 Localization System - Multi-language & RTL Support
Archivo: `09_Localization_System_Example.cs`

Paso a paso:
1. Carga catálogos de idiomas desde Resources o streaming asset bajo demanda.
2. Implementa sustitución de parámetros dinámicos usando `string.Format()` o `StringBuilder`.
3. Maneja plurales detectando cantidad y seleccionando forma correcta por idioma.
4. Detecta idiomas RTL (árabe, hebreo) ajustando dirección de layout automáticamente.
5. Actualiza todos los Text UI usando LINQ `GetAvailableLanguages()` al cambiar idioma.
6. Cachea catálogos cargados para evitar recargas repetidas.

## EN - 09 Localization System - Multi-language & RTL Support
File: `09_Localization_System_Example.cs`

Step by step:
1. Loads language catalogs from Resources or streaming asset on-demand.
2. Implements dynamic parameter substitution using `string.Format()` or `StringBuilder`.
3. Handles plurals by detecting quantity and selecting correct form per language.
4. Detects RTL languages (Arabic, Hebrew) automatically adjusting layout direction.
5. Updates all UI Text using LINQ `GetAvailableLanguages()` when changing language.
6. Caches loaded catalogs to avoid repeated reloads.

---

## ES - 10 Cinemachine Integration - Dynamic Cameras
Archivo: `10_Cinemachine_Example.cs`

Paso a paso:
1. Define múltiples cámaras virtuales Cinemachine con prioridad para cada contexto.
2. Implementa transiciones suaves elevando prioridad de cámara deseada.
3. Configura dampening (suavizado) personalizado para cada eje X/Y/Z.
4. Añade look-ahead para anticipar movimiento del objetivo.
5. Implementa efectos de shake usando `CinemachineImpulseSource` en impactos/eventos.
6. Soporta easing personalizado usando `Mathf.SmoothStep()` en transiciones.

## EN - 10 Cinemachine Integration - Dynamic Cameras
File: `10_Cinemachine_Example.cs`

Step by step:
1. Defines multiple Cinemachine virtual cameras with priority per context.
2. Implements smooth transitions by elevating priority of desired camera.
3. Configures custom dampening (smoothing) per axis X/Y/Z.
4. Adds look-ahead to anticipate target movement.
5. Implements shake effects using `CinemachineImpulseSource` on impacts/events.
6. Supports custom easing using `Mathf.SmoothStep()` in transitions.

---

## ES - 11 Mobile System - Touch Input & Hardware Access
Archivo: `11_Mobile_System_Example.cs`

Paso a paso:
1. Detecta gestos básicos midiendo distancia y velocidad de toque(s).
2. Implementa pan (drag) calculando `currentPosition - lastPosition`.
3. Implementa pinch detectando dos dedos en pantalla calculando distancia relativa.
4. Mapea gestos a acciones usando delegados/eventos.
5. Solicita permisos de hardware (vibración, giroscopio) en tiempo de ejecución.
6. Integra vibración nativa llamando `Handheld.Vibrate()` para feedback táctil.

## EN - 11 Mobile System - Touch Input & Hardware Access
File: `11_Mobile_System_Example.cs`

Step by step:
1. Detects basic gestures by measuring touch distance and velocity.
2. Implements pan (drag) by calculating `currentPosition - lastPosition`.
3. Implements pinch by detecting two fingers on screen calculating relative distance.
4. Maps gestures to actions using delegates/events.
5. Requests hardware permissions (vibration, gyroscope) at runtime.
6. Integrates native vibration calling `Handheld.Vibrate()` for tactile feedback.

---

## ES - 12 Sounds System - Audio Management & Pooling
Archivo: `12_Sounds_System_Example.cs`

Paso a paso:
1. Implementa pool de AudioSources usando Queue<AudioSource> para reutilización.
2. Configura atenuación 3D usando propiedades de AudioSource (minDistance, maxDistance).
3. Distribuye sonidos a grupos de mixer (Música, SFX, Voz) controlando volumen por categoría.
4. Implementa fade in/out usando corrutinas `WaitForSeconds` y `Mathf.Lerp()`.
5. Filtra solicitudes de sonido usando LINQ `Where()` por prioridad/categoría.
6. Limpia AudioSources al dejar de estar activos regresando al pool.

## EN - 12 Sounds System - Audio Management & Pooling
File: `12_Sounds_System_Example.cs`

Step by step:
1. Implements AudioSource pool using `Queue<AudioSource>` for reuse.
2. Configures 3D attenuation using AudioSource properties (minDistance, maxDistance).
3. Routes sounds to mixer groups (Music, SFX, Voice) controlling volume per category.
4. Implements fade in/out using coroutines `WaitForSeconds` and `Mathf.Lerp()`.
5. Filters sound requests using LINQ `Where()` by priority/category.
6. Cleans up AudioSources when no longer active returning to pool.

---

## ES - 13 Branch System - Decision Tree Manager
Archivo: `13_Branch_System_Example.cs`

Paso a paso:
1. Define árbol de decisión con nodos que contienen condiciones y ramas hijas.
2. Evalúa condiciones en orden filtrando ramas disponibles con `FirstOrDefault()`.
3. Cachea resultado de evaluación para evitar re-calcular en el mismo frame.
4. Invalida caché cuando cambia estado del juego (eventos del GameManager).
5. Soporta condiciones complejas combinando múltiples criterios dengan `&&` y `||`.
6. Dispara callback de rama seleccionada pasando contexto de evaluación.

## EN - 13 Branch System - Decision Tree Manager
File: `13_Branch_System_Example.cs`

Step by step:
1. Defines decision tree with nodes containing conditions and child branches.
2. Evaluates conditions in order filtering available branches with `FirstOrDefault()`.
3. Caches evaluation result to avoid re-calculating in same frame.
4. Invalidates cache when game state changes (GameManager events).
5. Supports complex conditions combining multiple criteria with `&&` and `||`.
6. Fires callback of selected branch passing evaluation context.

---

## ES - 14 Quiz System - Question Manager & Scoring
Archivo: `14_Quiz_System_Example.cs`

Paso a paso:
1. Carga preguntas desde ScriptableObject o base de datos aleatorizando orden.
2. Randomiza respuestas de cada pregunta usando LINQ `OrderBy(Random.value)`.
3. Compara respuesta seleccionada contra respuesta correcta para validar.
4. Calcula puntuación usando fórmula flexible (puntos, porcentaje, tiempo bonus).
5. Persiste resultados en PlayerPrefs o API backend para tracking histórico.
6. Proporciona feedback inmediato visual/audio indicando si respuesta fue correcta.

## EN - 14 Quiz System - Question Manager & Scoring
File: `14_Quiz_System_Example.cs`

Step by step:
1. Loads questions from ScriptableObject or database randomizing order.
2. Randomizes answers for each question using LINQ `OrderBy(Random.value)`.
3. Compares selected answer against correct answer for validation.
4. Calculates score using flexible formula (points, percentage, time bonus).
5. Persists results in PlayerPrefs or backend API for historical tracking.
6. Provides immediate visual/audio feedback indicating if answer was correct.

---

## ES - 15 VideoPlayer - Streaming & UI Integration
Archivo: `15_VideoPlayer_Example.cs`

Paso a paso:
1. Descarga video progresivamente desde URL usando corrutina `WWW` o `UnityWebRequest`.
2. Cachea video descargado en disco local para evitar re-descargar.
3. Sincroni subtítulos usando LINQ `FirstOrDefault()` encontrando entrada de tiempo más cercana.
4. Controla VideoPlayer (Play, Pause, Seek) desde UI vinculando eventos de UI a métodos.
5. Actualiza barra de progreso en Update usando `VideoPlayer.time / frame.durationInFrames`.
6. Maneja errores de descarga midiendo timeout y ofreciendo reintentos.

## EN - 15 VideoPlayer - Streaming & UI Integration
File: `15_VideoPlayer_Example.cs`

Step by step:
1. Progressively downloads video from URL using coroutine `WWW` or `UnityWebRequest`.
2. Caches downloaded video on local disk to avoid re-downloading.
3. Synchronizes subtitles using LINQ `FirstOrDefault()` finding closest time entry.
4. Controls VideoPlayer (Play, Pause, Seek) from UI binding UI events to methods.
5. Updates progress bar in Update using `VideoPlayer.time / frame.durationInFrames`.
6. Handles download errors by measuring timeout and offering retries.

---

## ES - 16 Version Control - Asset Versioning & Migration
Archivo: `16_VersionControl_Example.cs`

Paso a paso:
1. Calcula hash SHA256 de asset para detectar si cambió desde última versión.
2. Registra versión nueva en stack junto a hash, timestamp y changelog.
3. Valida compatibilidad verificando schema de versión anterior vs actual.
4. Implementa migración de datos cuando schema cambia (transformación de estructura).
5. Permite rollback regresando a última versión compatible en stack.
6. Soporta filtrado de assets versionados usando LINQ `Where()` por tipo/categoría.

## EN - 16 Version Control - Asset Versioning & Migration
File: `16_VersionControl_Example.cs`

Step by step:
1. Calculates SHA256 hash of asset to detect change since last version.
2. Registers new version in stack along hash, timestamp, and changelog.
3. Validates compatibility by checking previous vs current version schema.
4. Implements data migration when schema changes (structure transformation).
5. Allows rollback returning to last compatible version in stack.
6. Supports filtering versioned assets using LINQ `Where()` by type/category.

---

## ES - 17 GameManager - State Management & Lifecycle
Archivo: `17_GameManager_Example.cs`

Paso a paso:
1. Define máquina de estados para ciclo de juego (Menu, Loading, Gameplay, Pause, GameOver).
2. Implementa singleton con `DontDestroyOnLoad()` para persistencia entre escenas.
3. Carga escenas asincroniacamente usando `SceneManager.LoadSceneAsync()` con timeout.
4. Coordina subsistemas (Audio, UI, Physics) mediante eventos UnityEvent.
5. Guarda/carga estado del juego usando JSON y PlayerPrefs cada cierto tiempo.
6. Maneja ciclo de vida de aplicación (Quit, Pause en background) vía `OnApplicationFocus()`.

## EN - 17 GameManager - State Management & Lifecycle
File: `17_GameManager_Example.cs`

Step by step:
1. Defines state machine for game cycle (Menu, Loading, Gameplay, Pause, GameOver).
2. Implements singleton with `DontDestroyOnLoad()` for scene persistence.
3. Loads scenes asynchronously using `SceneManager.LoadSceneAsync()` with timeout.
4. Coordinates subsystems (Audio, UI, Physics) via UnityEvent events.
5. Saves/loads game state using JSON and PlayerPrefs at intervals.
6. Handles application lifecycle (Quit, Pause on background) via `OnApplicationFocus()`.

---