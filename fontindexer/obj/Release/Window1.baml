   M S B A M L   `   `   `  ����   fontindexer    fontindexer.Window1   \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    8x,http://schemas.microsoft.com/winfx/2006/xaml      � Title$  Font Indexer�� � 
ResizeMode$ NoResize=�$	��250��$	��600�� � 	Resources    cell �� 
TargetType �� Property �� Value 
 button��%.  &	        &	 h     �� " ��.���  � ��$
 Bold"���  � ��$ 14(���  � ��
 ��  ���  � ��$ 5q��� " ��.���  � ��$ Italic$���  � ��$
 Bold"���  � ��$ 12(���  � ��
 ��  ���  � ��$ 3q���  � ��$ Right=���  � ��$	 100��.��� $��10q� � ColumnDefinitions �� $	��100 ��� �� $	��120 � � RowDefinitions �� $��30 ��� $��30 ��� $��60 ��� .+��� $��0��#��[  $��0��$��Right=�$��Center=�$��Folder:���� $��1��$��0�� �� 
IsReadOnly . $��Center=�$
��Left=����� 	 �� Path$	 
FontFolder�
 �� UpdateSourceTrigger$
 PropertyChanged=� �� Mode$ TwoWay=��� -   $��0��$��2��#��[ $��Select���� $��0��#��[  $��1��$��Right=�$��Center=�$��Output file:���� $��1��$��1�� . $��Center=�$
��Left=����� $	 CsvFile�$
 PropertyChanged=�$ TwoWay=��� -   $��1��$��2��#��[ $��Select��� $��2��$��0��$��2��$��20�� � Minimum$ 0Q�$��	0 10 0 10q� � Maximum �� $	 ProMax�$
 PropertyChanged=�$ TwoWay=� � Value �� $	 ProVal�$
 PropertyChanged=�$ TwoWay=��� $��2��$��3��$��Center=�$	��100��$��Right=����� $	 ProText�$
 PropertyChanged=�$ TwoWay=��� $��3��$��0��$��2��$
��Left=�$��Bottom=�$7��12013-07-03 by Christian Blechert (http://fiae.ws)���� -   $��3��$��3��$��30��$��Do it!��$	��100��$��Right=�$��Bottom=�