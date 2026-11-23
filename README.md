# Sonic Renoved

Protótipo de **controlador 3D de alta velocidade no estilo Sonic Adventure**, feito em Unity com URP. O foco é a física de movimento: correr em loops e rampas alinhado à superfície, ganhar e perder velocidade em declives, pular, usar trampolins e o **homing attack** com mira automática. A fase de testes é a *Speed Highway*.

> Projeto de estudo de fã, sem fins comerciais. Sonic, os modelos e o cenário de *Sonic Adventure* pertencem à SEGA.

---

## Stack

| | |
|---|---|
| Engine | **Unity 2022.3.19f1 (LTS)** |
| Render | **URP 14** (perfis Performant, Balanced e High Fidelity) |
| Física | Rigidbody + raycasts / overlap |
| Input | Input Manager clássico (`Input.GetAxis`, `GetAxisRaw`) |
| Animação | Animator Controller (`sonicGraphic.controller`) com idle, corrida normal, boost, spin, spin jump, queda e trampolim |
| Outros | ProBuilder, Visual Effect Graph, TextMesh Pro, Timeline |

---

## Estrutura

```
Assets
├── script
│   ├── SonicScripts/SonicController.cs   # Física, velocidade, slopes, pulo, homing attack
│   ├── CameraController.cs               # Câmera orbital com up dinâmico e colisão
│   ├── tramp.cs                          # Trampolins / molas
│   └── UI/TargetUi.cs                    # Retícula do alvo do homing attack
├── Sonic/                                # Modelo, texturas e animações
├── Dreamcast - Sonic Adventure - Speed Highway/   # Cenário (OBJ em 3 partes)
└── Scenes/SampleScene.unity
```

---

## Destaques técnicos

### Alinhamento à superfície (loops, paredes e rampas)

O personagem não usa a gravidade "para baixo" como referência. Ele segue a **normal do chão**:

1. Um raycast sai de `startPosray` na direção `-transform.up`, ou seja, do personagem e não do mundo. Isso faz o chão continuar sendo detectado dentro de loops e paredes.
2. A partir da normal do hit e do `forward` da câmera, monta uma base local com produtos vetoriais:

   ```csharp
   Vector3 rightDir   = Vector3.Cross(cam.forward, normal);
   Vector3 forwardDir = Vector3.Cross(rightDir, normal);
   Vector3 dirToWalk  = -forwardDir * vertical + -rightDir * horizontal;
   ```

   Assim o input é sempre relativo à câmera, mas projetado no plano tangente da superfície.
3. A rotação é composta por `Quaternion.LookRotation(dirToWalk, normal)` para a direção e `Quaternion.FromToRotation(transform.up, normal)` para alinhar o *up*, ambos suavizados com `Lerp`.
4. No ar, o *up* volta gradualmente para `Vector3.up`.

Os raycasts e vetores ficam desenhados com `Debug.DrawRay` para depuração.

### Velocidade e física de declive

A velocidade é um escalar (`currentSonicSpeed`) aplicado sempre na direção `transform.forward`. Isso garante que o personagem "grude" e siga a curvatura da pista.

- **Ângulo do declive:** `Vector3.Angle(transform.up, Vector3.up)`. Acima de 15°, o sinal de `Dot(Vector3.up, transform.forward)` diz se é subida (> 0) ou descida (< 0).
- **Descida:** acelera proporcionalmente à inclinação (`slopeAssistanceAce`) e aumenta o limite de velocidade até `maxSpeedSonicLimit`.
- **Subida:** desacelera (`slopeAssistanceDec`) e reduz o teto de velocidade.
- **Sem input em declive:** o personagem gira para a direção da descida (`ProjectOnPlane(Vector3.down, normal)`) e escorrega.
- **No plano:** aceleração e desaceleração lineares, com o teto voltando suavemente para o valor base.
- **No ar:** desaceleração própria (`AirdecelerationSonic`).
- Acima de uma velocidade mínima, aplica uma força extra contra a normal (`-transform.up * 4`) para não descolar do chão em alta velocidade.

### Pulo

- Velocidade inicial calculada pela altura desejada: `v = √(2·g·h)`, somada ao vetor de movimento atual para manter o momento.
- O pulo é aplicado ao longo do `transform.up`, ou seja, perpendicular à superfície (é possível pular de paredes e loops).
- Libera o homing attack depois de um pequeno atraso (`Invoke`).

### Homing attack

1. A cada `FixedUpdate`, procura objetos com a tag `attackers` e escolhe o **mais próximo** comparando `sqrMagnitude` (sem raiz quadrada), dentro do raio `distanceTarget`.
2. Se o jogador aperta pulo no ar, o Sonic é lançado a velocidade constante na direção do alvo. Sem alvo, ele faz um *air dash* para a frente por 0,2 s.
3. O ataque termina ao chegar a 3,5 unidades do alvo.
4. `TargetUi` posiciona uma retícula em world-space sobre o alvo atual, orientada para a câmera (billboard).

### Trampolins

`tramp.cs` detecta o Sonic com `Physics.OverlapSphere` filtrado por layer. Ao detectar, zera a velocidade, aplica um impulso (`ForceMode.Impulse`) na direção `transform.up` do trampolim (que pode estar inclinado) e, opcionalmente, zera a velocidade horizontal. Há gizmo no editor para posicionar a área de ativação.

### Câmera orbital

`CameraController` mantém um pivô que segue o personagem e:

- **Alinha o *up* da câmera ao *up* do personagem** (`FromToRotation` + `Lerp`), para a câmera acompanhar loops e paredes.
- Faz yaw ao redor do `up` local e pitch ao redor do `right` local, com **limite de ângulo** medido entre a direção da câmera e o `up` do jogador.
- **Evita atravessar o cenário** com um raycast do pivô até a câmera: se houver colisão, aproxima a câmera até o ponto de impacto, e senão volta para a distância padrão. Os dois movimentos são suavizados.

### Integração com o Animator

Parâmetros atualizados a cada frame: `isGround`, `movementSpeed` (blend entre corrida normal e boost) e `inSpinDash`. Estados disparados diretamente: `spinJump`, `jumpTramplin`.

---

## Como rodar

1. Abra o projeto com **Unity 2022.3.19f1**.
2. Abra `Assets/Scenes/SampleScene.unity` e dê Play.

| Ação | Tecla |
|---|---|
| Mover | WASD / setas |
| Câmera | Mouse |
| Pular | Espaço |
| Homing attack | Espaço no ar (com alvo destacado) |
