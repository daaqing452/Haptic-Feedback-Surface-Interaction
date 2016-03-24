# MegAug README

### import
```
from megaug.augmentaions import *
```


### AugPipeline
##### 成员变量
* `megbrain.comp_graph` **cg**

* `megbrain.comp_node `**cn**
    
* `float` **seed** = time.time()

    > megbrain的随机种子

##### 成员函数
* `AugImage` **new_image**(name=None, contain_mask=False, contain_landmark=False, is_main=True)

    > 新建一个图片
    
    > name：图片名字
    
    > contain_mask：是否包含mask
    
    > contain_landmark：是否包含landmark
    
    > is_main：是否为主图，一个AugPipeline有且仅有一张主图

* `void` **compile**()

    > 将建好的流水线编译
    
    > run所需的输入和run之前需要赋值的变量会打印在屏幕上

* `void` **set_value**(name, value)

    > 将一个名为name的变量赋值value

* `numpy.ndarray` **run**(*args)

    > 将所需的输入参数传入得到最终结果


### AugImage
##### 成员变量
* **name**
* **image**
* **mask**
* **landmark**

    > name：图片名字
    
    > 图片格式为(1,c,h,w)
    
    > mask格式为(1,1,h,w)
    
    > landmark格式为(x,2)

##### 成员函数
* `AugImage` **augmented**(augmentation, unassign=False)

    > 将一个增强augmentation应用到图片上


### AffineNoise
##### 成员变量
* `float` **rotate_mean** = 0.0
* `float` **rotate_std** = 0.2
    
    > 旋转噪声平均值和幅度

    > rotate ∈ rotate_mean ± 1/2 * rotate_std

* `float` **scale_mean** = 1.0
* `float` **scale_std** = 0.2

    > 缩放噪声平均值和幅度
    
    > scale ∈ scale_mean ± 1/2 * scale_std

* `(float,float)` **translation_mean** = (0.0, 0.0)
* `(float,float)` **translation_std** = (0.2, 0.2)

    > 位移噪声平均值和幅度
    
    > translation_x ∈ translation_mean[0] ± 1/2 * translation_std[0]
    
    > translation_y ∈ translation_mean[1] ± 1/2 * translation_std[1]

* `megbrain.opr.WarpPerspectiveBorderMode` **border_mode** = CONSTANT

    > 边界类型

* `float` **border_value** = 0.0

    > 边界填充颜色

* `megbrain.opr.WarpPerspectiveInterpMode` **interp_mode** = LINEAR

    > 差值类型

##### 成员函数
* **__init__**(name=None)


### GammaNoise
##### 成员变量
* `int` **channel** = None

    > channel == 1 则三个颜色通道使用同一噪声
    
    > channel == 3 则三个颜色通道分别使用不同噪声
    
    > channel == None 则需要在run时传入一个和被增强图大小相同的矩阵m，格式为(1,1,h,w)

* `float` **gamma_mean** = 0.5
* `float` **gamma_std** = 1.0

    > gamma噪声的指数参数
    
    > gamma = gamma_mean + gamma_std
