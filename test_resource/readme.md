## 使用ResourceDictionary
1. blend软件点击右边Resources标签 然后看到该文件名字样式右边的编辑按钮点击 就会在Design页面中显示当前元素

## 依赖属性优先级

```c#
<Button Background="Red">
      <Button.Style>
        <Style TargetType="{x:Type Button}">
          <Setter Property="Background" Value="Green"/>
          <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
              <Setter Property="Background" Value="Blue" />
            </Trigger>
          </Style.Triggers>
        </Style>
      </Button.Style>
</Button><!-- red? green? blue? -->
```
ref: `https://github.com/dotnet/docs.zh-cn/blob/live/docs/framework/wpf/advanced/dependency-property-value-precedence.md`

翻译:
```
属性系统强制。 有关强制的详细信息，请参阅本主题后面的强制、动画和基值。

活动动画或具有 Hold 行为的动画。 为了获得任何实用效果，属性的动画必须优先于基（未动画）值，即使该值是在本地设置的也是如此。 有关详细信息，请参阅本主题后面的强制、动画和基值。

本地值。 可能的便利性"包装器"属性，这也相当于设置中的某个属性或属性的元素是通过设置本地值XAML，或通过调用SetValueAPI使用的特定实例的属性。 如果使用绑定或资源来设置本地值，则每个值都按照直接设置值的优先级顺序来应用。

TemplatedParent 模板属性。 元素具有TemplatedParent是创建模板的一部分 (ControlTemplate或DataTemplate)。 有关何时应用此原则的详细信息，请参阅本主题后面的 TemplatedParent。 在模板中，按以下优先级顺序应用：

    触发从TemplatedParent模板。

    属性集 (通常通过XAML属性) 中TemplatedParent模板。

隐式样式。 仅应用于 Style 属性。 Style 属性是由任何样式资源通过与其类型匹配的键来填充的。 该样式资源必须存在于页面或应用程序中；查找隐式样式资源不会进入到主题中。

样式触发器。 来自页面或应用程序的样式中的触发器（这些样式可以是显式或隐式样式，但不是来自优先级较低的默认样式）。

模板触发器。 来自样式中的模板或者直接应用的模板的任何触发器。

样式资源库。 中的值Setter来自页面或应用程序的样式中。

默认（主题）样式。 有关何时应用此样式以及主题样式如何与主题样式中的模板相关的详细信息，请参阅本主题后面的默认（主题）样式。 在默认样式中，按以下优先级顺序应用：

    主题样式中的活动触发器。

    主题样式中的资源库。

继承。 有几个依赖属性从父元素向子元素继承值，因此不需要在应用程序中的每个元素上专门设置这些属性。 有关详细信息，请参阅属性值继承。

来自依赖属性元数据的默认值。 任何给定的依赖属性都可能有一个默认值，它由该特定属性的属性系统注册来确定。 而且，继承依赖属性的派生类可以选择按照类型重写该元数据（包括默认值）。 有关详细信息，请参阅依赖属性元数据。 因为继承是在默认值之前检查的，所以对于继承的属性，父元素的默认值优先于子元素。 因此，如果任何地方都没有设置可继承的属性，将使用在根元素或父元素中指定的默认值，而不是子元素的默认值。
```
