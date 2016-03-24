# MegAug README

### import
```
from megaug.augmentaions import *
```

### AugPipeline
```
ap = AugPipeline()
```
##### 成员变量
* **cg**

    > comp_grapy()

* **cn**

> comp_node('gpu0')

* **seed**

> megbrain的随机种子

##### 成员函数
*   `AugImage` **new_image**(name=None, contain_mask=False, contain_landmark=False, is_main=True)
    > 新建一个图片
    * name：图片名字
    * contain_mask：是否包含mask
    * contain_landmark：是否包含landmark
    * is_main：是否为主图，一般一个AugPipeline有一张主图
*   compile()
    > 打印在compile是
*   
