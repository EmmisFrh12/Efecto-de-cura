# Computación Gráfica 2026-6
## Entrega 1: Efecto de Curación

**Estudiantes:** Juan Esteban Medina, Emmanuel Gamboa y Juan David Negrete 

---

### Descripción del Proyecto
Este proyecto consiste en la creación de un efecto visual (VFX) de curación (Healing Effect) dentro de Unity.

### Estructura del Efecto (Ciclo de Vida)
El comportamiento del sistema está dividido en tres estados principales:
* **Inicio:** Aparición progresiva de la energía, construcción de las auras y nacimiento de la cruz.
  
  <img width="429" height="368" alt="image" src="https://github.com/user-attachments/assets/9b71cb99-708f-4971-961d-33e1d0bedcdc" />

* **Loop:** Animación continua de sanación donde la cruz oscila y rota, acompañada del flujo constante de partículas ascendentes y vórtices circulares.
  
  <img width="266" height="275" alt="image" src="https://github.com/user-attachments/assets/1bb3585b-e782-477b-8519-989e89c6105b" />

* **Final:** Disipación de los elementos, desvanecimiento de la luz y retracción del efecto.
  
  <img width="207" height="289" alt="image" src="https://github.com/user-attachments/assets/a0f5f6eb-b502-4103-87b0-18f11c405bf6" />


### Componentes Visuales
* **Meshes (Mallas):**
  * **Cruz central:** El elemento principal; rota, cambia de tamaño y oscila constantemente en el eje vertical.
  * **Esfera:** Genera un aura luminosa alrededor de la cruz.
  * **Cilindros (2):** Ubicados en la parte inferior, actúan como base proyectando dos auras circulares bajo la cruz, una mas alta en el centro.
* **Trails (Estelas):** 
  * Sistema de 2 vórtices que orbitan y van cayendo hacia el centro. Para dar mayor riqueza visual, estos elementos se duplicaron, se les configuró un *delay* (retraso en la emisión) y se les ajustó el color.
* **Billboards:** 
  * Destellos con textura en forma de cruz.
  * Destellos verticales que se desplazan fluidamente de abajo hacia arriba.
  * Destellos de luz que nacen de forma radial alrededor de la cruz central.
* **Partículas Base:** 
  * Burbujas de luz flotantes que ascienden para dar la sensación de un área energizada.

### Herramientas y Técnicas Utilizadas
* **Sistema de Partículas (Particle System):** Motor principal de Unity para la orquestación y sincronización de los emisores (burbujas, destellos, trails y control de mallas).
* **Materiales Unlit:** Implementados para asegurar que la luminosidad de los destellos y auras mantenga su intensidad y color independientemente de la iluminación del entorno.
* **Texturas 2D:** Creados en Photoshop y/o la pagina web [EffectTextureMaker](https://mebiusbox.github.io/contents/EffectTextureMaker/)
* **Shader Graph:** Desarrollo de shaders personalizados para el comportamiento y aspecto de los materiales dentro de los sistemas.

 ### Efecto Completo
 

https://github.com/user-attachments/assets/e58d942d-06d0-4a1b-8201-441a7aa33ab0


