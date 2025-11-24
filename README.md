# 🎳 Bowling Strike – Mini Juego en Unity

**Bowling Strike** es un juego de bolos desarrollado con **Unity 6 (Built-in)** pensado para móvil. El objetivo es simple: **apuntar, cargar el lanzamiento y derribar tantos bolos como puedas**.

---

## 🕹️ ¿Cómo se juega?

### **1. Posicionamiento**
- Arrastra la bola horizontalmente antes de lanzar.
- Puedes moverla dentro del carril, pero sin salirte de los límites.

### **2. Carga del lanzamiento**
- Pulsa y arrastra hacia atrás para cargar potencia.
- Cuanta más distancia arrastres, mayor fuerza tendrá el lanzamiento.
- Suelta para lanzar.

### **3. Impacto y puntuación**
- La bola avanza por la pista y colisiona con los bolos.
- El sistema detecta automáticamente los bolos derribados.
- La puntuación se muestra en tiempo real.

---

## 🎥 Cámaras del juego

El juego alterna entre tres cámaras según la fase:

- **Vista superior** → para el posicionamiento inicial.
- **Vista previa de lanzamiento** → durante la carga.
- **Cámara seguidora** → sigue la bola en su recorrido.

---

## 🎯 Características principales

- **Sistema de puntuación automático.**
- **Detección de bolos derribados** mediante comportamiento físico y colisiones.
- **Cámara dinámica** que sigue la bola durante el lanzamiento.
- **UI sencilla** con instrucciones y marcador en pantalla.

---

## 📦 Estructura del proyecto

- **GameManager**  
  Gestiona todo el flujo del juego: cámaras, UI, lanzamiento, movimiento horizontal, creación de bolos y reinicio.

- **BoloController**  
  Controla la detección de cada bolo, cuándo está derribado y cuándo sumar puntos.

---

## 📱 Plataforma

- **Dispositivos móviles**  
  Controles adaptados a entrada táctil.

---

## 🚀 Objetivo del juego

Derribar tantos bolos como sea posible en un único lanzamiento.  
La partida finaliza cuando la bola sale de la pista o se detiene, mostrando la puntuación total antes de poder reiniciar.

---

¡Listo para jugar y mejorar tu puntería! 🎳🔥
