# MegAug README

### import
```
from megaug.augmentaions import *
```

### AugPipeline
##### 成员变量
* *megbrain.comp_graph* **cg**

* `megbrain.comp_node `**cn**
    
* `float` **seed**

    > megbrain的随机种子

##### 成员函数
* `AugImage` `**new_image**(name=None, contain_mask=False, contain_landmark=False, is_main=True)`

    > 新建一个图片
    
    * name：图片名字
    * contain_mask：是否包含mask
    * contain_landmark：是否包含landmark
    * is_main：是否为主图，一般一个AugPipeline有一张主图

* `void` **compile**()

    > 将建好的流水线编译
    > run所需的输入和run之前需要赋值的变量会打印在屏幕上

* `void` **set_value**(name, value)

    > 将一个名为name的变量赋值value

* `numpy.ndarray` **run**(*args)

    > 将所需的输入参数传入得到最终结果
    > 图片格式为(1,c,h,w)
    > landmark格式为(x,2)

### AugImage
##### 成员函数
* `AugImage` **augmented**(augmentation, unassign=False)

    > 将一个增强augmentation应用到图片上
    

### AffineNoise
##### 成员变量
* `float` **rotate_mean**
* `float` **rotate_std**
    
    > 旋转噪声平均值和幅度
    > rotate_mean ± 1/2 * rotate_std

* `float` **scale_mean**
* `float` **scale_std**

    > 缩放噪声平均值和幅度
    > scale_mean ± 1/2 * scale_std
