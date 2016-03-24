# MegAug README

### import
```
from megaug.augmentaions import *
```

### AugPipeline
##### 成员变量
* **cg**

    > comp_grapy()

* **cn**

    > comp_node('gpu0')
    
* **seed**

    > megbrain的随机种子

##### 成员函数
* `AugImage` **new_image**(name=None, contain_mask=False, contain_landmark=False, is_main=True)

    > 新建一个图片
    
    * name：图片名字
    * contain_mask：是否包含mask
    * contain_landmark：是否包含landmark
    * is_main：是否为主图，一般一个AugPipeline有一张主图

* `void` **compile**()

    > 将建好的流水线编译
    > run所需的输入和run之前需要赋值的变量会打印在屏幕上

* `void` **set_value**(name, value)

    > 将一个名为name的变量赋值

* `numpy.ndarray` **run**(*args)

    > 将所需的输入参数传入得到最终结果

