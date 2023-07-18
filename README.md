
## Chapter 2. Draw Call
### Dynamic Batching
<img src = "https://github.com/eunho5751/CustomSRP/assets/29402080/926c0e62-cd0d-4ee7-b263-cc044d98a1e1" width="75%" height="75%">

**4 Batches** = **3** Batches by Cubes' Dynamic Batching + **1** Batch by Skybox
<br/>
**4 SetPass calls** = **3** SetPass calls by each cube's shared material + **1** SetPass call by Skybox material

### GPU Instancing
<img src = "https://github.com/eunho5751/CustomSRP/assets/29402080/0e3fffc3-28ce-4990-bbb6-fc80daf4c776" width="50%" height="50%">


**4 Batches** = **3** Batches by Cubes' GPU Instancing + **1** Batch by Skybox
<br/>
**4 SetPass calls** = **3** SetPass calls by each cube's shared material + **1** SetPass call by Skybox material

### SRP Batcher
<img src = "https://github.com/eunho5751/CustomSRP/assets/29402080/2cb001ac-e717-472d-8d8f-d17fe7b31338" width="75%" height="75%">

**16 Batches** = **15** Batches by Cubes + **1** Batch by Skybox
<br/>
**2 SetPass calls** = **1** SetPass call by Cube materials' shader variant + **1** SetPass call by Skybox material

## Chapter 3. Directional Lights
### Render Types
<img src = "https://github.com/eunho5751/CustomSRP/assets/29402080/461b4fd0-2ef9-4bdc-95d1-56f5de303b63" width="19.5%" height="19.5%">
<img src = "https://github.com/eunho5751/CustomSRP/assets/29402080/021a0459-1e1c-48bd-8480-31c6ea9633dd" width="20%" height="20%">
<img src = "https://github.com/eunho5751/CustomSRP/assets/29402080/813bdae1-2606-483d-aa13-2761eac3c851" width="19%" height="19%">
<img src = "https://github.com/eunho5751/CustomSRP/assets/29402080/bd9e0188-cf36-4d4e-b5b6-d31887fb572e" width="18.7%" height="18.7%">


_1._ **Opaque** <br/>
_2._ **Clip** <br/>
_3._ **Fade** (Specular Reflection is affected by alpha) <br/>
_4._ **Transparent** (Specular Reflection is not affected by alpha) → Premultiplied Alpha <br/>

### GPU Instancing with Lit material
![image](https://github.com/eunho5751/CustomSRP/assets/29402080/e86e201a-eb2c-4ad5-ba8c-59a4707e9dd6)
