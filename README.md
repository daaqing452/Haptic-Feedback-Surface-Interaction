# MegAug使用文档

## import
```
from megaug.augmentaions import *
```


## AugPipeline

> 图片增强流水线，必须存在

#### 成员变量
* `megbrain.comp_graph` **cg**

* `megbrain.comp_node `**cn**
    
* `float` **seed** = time.time()

    > megbrain的随机种子

#### 成员函数
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


## AugImage

> 图片类

#### 成员变量
* `str` **name**
* **image**

    > 格式为(1,c,h,w)

* **mask**

    > 格式为(1,1,h,w)

* **landmark**
    
    > 格式为(x,2)

#### 成员函数
* `AugImage` **augmented**(augmentation, unassign=False)

    > 将一个增强augmentation应用到图片上


## AffineNoise

> 仿射变换噪声

#### 成员变量
* `str` **name**
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

* `float` **border_value** = 0.0

    > 边界填充颜色

#### 成员函数
* **\_\_init\_\_**(name=None)


## GammaNoise

> Gamma噪声

#### 成员变量
* `str` **name**
* `int` **channel** = None

    > channel == 1 则三个颜色通道使用同一噪声
    
    > channel == 3 则三个颜色通道分别使用不同噪声
    
    > channel == None 则需要在run时传入一个和被增强图大小相同的矩阵m，格式为(1,1,h,w)

* `float` **gamma_mean** = 0.5
* `float` **gamma_std** = 1.0

    > gamma噪声指数的平均值和幅度
    
    > gamma ∈ gamma_mean + 1/2 * gamma_std
    
#### 成员函数
* **\_\_init\_\_**(channel=None, name=None)


## GaussianNoise

> 高斯噪声

#### 成员变量
* `str` **name**
* `float` **gaussian_mean** = 0
* `float` **gaussian_std** = 10

    > gaussian噪声的平均值和幅度
    
    > gaussian ∈ gaussian_mean 1/2 * gaussian_std

#### 成员函数
* **\_\_init\_\_**(gaussian_mean=0, gaussian_std=10, name=None)


## Flip

> 翻转

#### 成员变量
* `str` **name**
* `str` **flip_axis**

    > flip_axis == 'X' 则进行水平翻转
    > flip_axis == 'Y' 则进行垂直翻转

#### 成员函数
* **\_\_init\_\_**(flip_axis='X', name=None)


## Graying

> 将3-channel的彩色图变成3-channel的灰度图

#### 成员变量
* `str` **name**

#### 成员函数
* **\_\_init\_\_**(name=None)


## InterpolationBlur

> 插值模糊，将图缩小再放大

#### 成员变量
* `str` **name**
* `str` **interp_mode** = 'LINEAR'

    > 插值类型
    
    > 目前只支持'LINEAR'

* `(float,float)` **resize_ratio** = (0.5,0.5)
* `(float,float)` **resize_shape** = None

    > resize_shape != None 则将图缩小到resize_shape大小
    
    > resize_shape == None 则将图缩小到resize_ratio比例大小

#### 成员函数
* **\_\_init\_\_**(resize_ratio=(0.5,0.5), resize_shape=None, name=None)


## GaussianBlur

> 高斯模糊

#### 成员变量
* `str` **name**
* `str` **blur_method** = 'AVERAGE'

    > 模糊的filter类型
    
    > 支持'AVERAGE'（同一参数）、'RANDOM'（随机分布）

* `float` **filter_h** = 5
* `float` **filter_w** = 5

    > filter大小

* `float` **pad_h** = 2
* `float` **pad_w** = 2

    > padding大小

* `float` **stride_h** = 1
* `float` **stride_w** = 1 

    > stride大小

#### 成员函数
* **\_\_init\_\_**(name=None)


## ShelterLibrary

> 遮挡

#### 成员变量
* `str` **name**
* `str` **shelter_type** = 'NONE'

    > 遮挡类型
    
    > 目前支持'GLASS'（眼镜）、'HAIR'（头发）

#### 成员函数
* **\_\_init\_\_**(shelter_type='NONE', name=None)
