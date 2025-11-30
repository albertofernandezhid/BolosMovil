![Unity](https://img.shields.io/badge/Engine-Unity%206-black?logo=unity)
![Status](https://img.shields.io/badge/Status-In%20Progress-yellow)
![License](https://img.shields.io/badge/License-MIT-green)
![CLA Required](https://img.shields.io/badge/CLA-Required-blue.svg)
![Android](https://img.shields.io/badge/Platform-Android-green?logo=android)

# 🎳 Bowling Riot: Simulador de bolos para móvil

**Bowling Riot** es una simulación de bolos desarrollada en **Unity 6 (Built-in)** y optimizada para dispositivos móviles.  
El proyecto se centra en ofrecer una experiencia fluida e intuitiva gracias a controles táctiles precisos, cámaras dinámicas y un sistema de físicas realista para el impacto y la puntuación.

---

## 🎥 Gameplay (YouTube)

[![Gameplay en YouTube](https://img.youtube.com/vi/Tn2CIoJNplA/maxresdefault.jpg)](https://youtu.be/Tn2CIoJNplA)

---

## ⚙️ Arquitectura del proyecto

El flujo del juego y la lógica de negocio están organizados mediante una arquitectura clara basada en componentes, entre ellos:

| Componente      | Responsabilidad principal |
|----------------|----------------------------|
| **GameManager** | Orquestación del flujo de la partida, control de estados (Posicionamiento, Carga, Lanzada), gestión de cámaras, interacción con la UI y reinicio de la escena. |
| **ScoreManager** | Gestión del sistema de puntuación simplificado del bowling. |
| **BallLauncher** | Control de la entrada táctil del jugador (arrastre horizontal y carga de potencia) y aplicación de la fuerza inicial a la bola. |
| **PinManager** | Control del set de bolos, detección automática de bolos derribados y reposicionamiento/respawn entre rondas. |
| **BoloController** | Determinar el estado individual de cada bolo (de pie vs. caído) basándose en su ángulo y física. |
| **MenuManager** | Gestión de menús, navegación, opciones y configuración inicial. |

---

## 🎮 Fases y mecánicas de juego

El flujo principal del juego se divide en tres fases, cada una acompañada de una cámara específica:

### 1. **Posicionamiento (Vista superior)**
El jugador arrastra la bola horizontalmente sobre la pista para elegir el punto inicial del lanzamiento.

### 2. **Carga (Vista previa de lanzamiento)**
- El jugador arrastra hacia atrás en el eje Z para cargar potencia.  
- La distancia de arrastre determina la fuerza aplicada.  
- El lanzamiento se realiza al soltar la pantalla.

### 3. **Lanzada (Cámara seguidora)**
- La bola avanza por la pista bajo las físicas de Unity.  
- El impacto determina cuántos bolos son derribados.  
- El turno termina cuando la bola se detiene o atraviesa el colisionador final/canaleta.

---

## 🌐 Características destacadas

- **Física realista de bolos**  
  Colisiones precisas, masa ajustada y comportamiento natural de caída gracias al motor de físicas de Unity.

- **Controles táctiles intuitivos**  
  Sistema de arrastre horizontal + carga de potencia que hace el lanzamiento sencillo pero con profundidad.

- **Cámaras dinámicas**  
  Tres cámaras integradas: vista superior, vista de preparación y cámara seguidora fluida durante la rodadura.

- **Sistema de puntuación oficial**  
  Implementación simplificada del reglamento del bowling.

- **Gestión automática de bolos**  
  Detección de bolos derribados, reinicio de la mesa y reposicionamiento entre rondas.

- **Optimización para móviles**  
  Texturas comprimidas, físicas ajustadas y rendimiento uniforme en dispositivos de gama media.

- **UI clara y minimalista**  
  Interfaz pensada para móviles con botones grandes, paneles limpios y navegación sencilla.

- **Compatibilidad universal**  
  Construido con el render pipeline Built-in para asegurar compatibilidad en la mayoría de dispositivos Android.

---

## 📦 Tecnologías utilizadas
- **Unity 6 (6000.0.60f1) – Built-in Render Pipeline**  
- C# para lógica de juego  

---

## 📱 Plataforma objetivo
- **Android**

---

## 🚀 Estado del proyecto
En desarrollo activo. Se añadirán nuevas características, mejoras de físicas y opciones de personalización.