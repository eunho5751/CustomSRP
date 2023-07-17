
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
