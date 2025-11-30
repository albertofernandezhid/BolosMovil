# 🎳 Bowling Riot: Simulador de Bolos para Móvil

**Bowling Strike** es una simulación de bolos desarrollada en **Unity 6 (Built-in)** y optimizada para dispositivos móviles.  
El proyecto se centra en ofrecer una experiencia fluida e intuitiva gracias a controles táctiles precisos, cámaras dinámicas y un sistema de físicas realista para el impacto y la puntuación.

---

## ⚙️ Arquitectura del Proyecto

El flujo del juego y la lógica de negocio están organizados mediante una arquitectura clara basada en componentes, entre ellos:

| Componente      | Responsabilidad Principal |
|----------------|----------------------------|
| **GameManager** | Orquestación del flujo de la partida, control de estados (Posicionamiento, Carga, Lanzada), gestión de cámaras, interacción con la UI y reinicio de la escena. |
| **ScoreManager** | Gestión del sistema de puntuación simplificado del bowling. |
| **BallLauncher** | Control de la entrada táctil del jugador (arrastre horizontal y carga de potencia) y aplicación de la fuerza inicial a la bola. |
| **PinManager** | Control del set de bolos, detección automática de bolos derribados y reposicionamiento/respawn entre rondas. |
| **BoloController** | Determinar el estado individual de cada bolo (de pie vs. caído) basándose en su ángulo y física. |
| **MenuManager** | Gestión de menús, navegación, opciones y configuración inicial. |

---

## 🎮 Fases y Mecánicas de Juego

El flujo principal del juego se divide en tres fases, cada una acompañada de una cámara específica:

### 1. **Posicionamiento (Vista Superior)**
El jugador arrastra la bola horizontalmente sobre la pista para elegir el punto inicial del lanzamiento.

### 2. **Carga (Vista Previa de Lanzamiento)**
- El jugador arrastra hacia atrás en el eje Z para cargar potencia.  
- La distancia de arrastre determina la fuerza aplicada.  
- El lanzamiento se realiza al soltar la pantalla.

### 3. **Lanzada (Cámara Seguidora)**
- La bola avanza por la pista bajo las físicas de Unity.  
- El impacto determina cuántos bolos son derribados.  
- El turno termina cuando la bola se detiene o atraviesa el colisionador final/canaleta.

---

## 🌐 Características Destacadas

- **Física Realista de Bolos**  
  Colisiones precisas, masa ajustada y comportamiento natural de caída gracias al motor de físicas de Unity.

- **Controles Táctiles Intuitivos**  
  Sistema de arrastre horizontal + carga de potencia que hace el lanzamiento sencillo pero con profundidad.

- **Cámaras Dinámicas**  
  Tres cámaras integradas: vista superior, vista de preparación y cámara seguidora fluida durante la rodadura.

- **Sistema de Puntuación Oficial**  
  Implementación simplificada del reglamento del bowling.

- **Gestión Automática de Bolos**  
  Detección de bolos derribados, reinicio de la mesa y reposicionamiento entre rondas.

- **Optimización para Móviles**  
  Texturas comprimidas, físicas ajustadas y rendimiento uniforme en dispositivos de gama media.

- **UI Clara y Minimalista**  
  Interfaz pensada para móviles con botones grandes, paneles limpios y navegación sencilla.

- **Compatibilidad Universal**  
  Construido con el render pipeline Built-in para asegurar compatibilidad en la mayoría de dispositivos Android.

---

## 📦 Tecnologías Utilizadas
- **Unity 6 (6000.0.60f1) – Built-in Render Pipeline**  
- C# para lógica de juego  

---

## 📱 Plataforma Objetivo
- **Android**

---

## 🚀 Estado del Proyecto
En desarrollo activo. Se añadirán nuevas características, mejoras de físicas y opciones de personalización.
