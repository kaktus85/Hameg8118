   M S B A M L   `   `   `  ����D  @Hameg8118, Version=1.1.1.0, Culture=neutral, PublicKeyToken=null%clr-namespace:Hameg8118	Hameg8118  5   	       Hameg8118.HowToConnect   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:Hameg8118     5   	   -   5          � Title$  How To Connect to HM8118��5   	   $	��390��6*   $	��550��67    � 
ResizeMode$ NoResize=�6C    � Icon$ icon.ico��6Y   .��5	      � .+�5
   
   �� -   5         ��Name$ 	textBlock��5
      $��
20,10,20,0q�6'   $
��Wrap=�6;   $	��Top=�6O   $	��307��6g    �� TextAlignment$ Justify=�6t   �� 6�   $
��Bold"�6�   $��16(�6�   $A�;This program supports connection only via serial (COM) port��6�   �� 6�   �� 6  $
��Bold"�6  �� 6  �� 6&  $	�1) ��6*  �� 67  $'�!Connect the device via USB cable.��6;  �� 6f  �� 6r  �� 6x  �� 6�  $	�2) ��6�  �� 6�  $	�In ��6�  �� 6�  $
��Bold"�6�  $��Italic$�6�  $�Device manager��6�  �� 6�  $�, find��6�  �� 6�  $
��Bold"�6�  $��Italic$�6  $� ��6$  �� 6/  $
��Bold"�63  $��Italic$�6E  $�HAMEG HO820 USB��6X  �� 6q  $
� in ��6u  �� 6�  $
��Bold"�6�  $��Italic$�6�  $ �HAMEG Measurement Devices ��6�  �� 6�  $	�or ��6�  �� 6�  $
��Bold"�6�  $��Italic$�6�  $�Other devices��6
  �� 6!  $� ��6%  �� 60  $�menu. ��64  �� 6D  $�If it resides in ��6H  �� 6c  $
��Bold"�6g  $��Italic$�6y  $�Other devices��6�  �� 6�  $�, click ��6�  �� 6�  $
��Bold"�6�  $��Italic$�6�  $�Update Driver Software…��6�  �� 6  $0�* and install the proprietary drivers from ��6  �� 6;  $�Rohde & Schwarz��6?  �� 6\  $� (click ��6`  �� 6r  $
��Bold"�6v  $��Italic$�6�  $�Download driver��6�  �� 6�  $� button below).��6�  �� 6�  �� 6�  �� 6�  �� 6�  $	�3) ��6�  �� 6   $7�1Then, when the drivers have been installed, open ��6  �� 6?  $
��Bold"�6C  $��Italic$�6U  $�HAMEG HO820 USB��6h  �� 6�  $� properties and select ��6�  �� 6�  $
��Bold"�6�  $��Italic$�6�  $�Advanced��6�  �� 6�  $�. Check ��6�  �� 6�  $
��Bold"�6�  $��Italic$�6  $�Load VCP��6   �� 62  $� and confirm. ��66  �� 6N  $G�ADisconnect the USB cable from the device and reconnect it again. ��6R  �� 6�  �� 6�  �� 6�  �� 6�  �� 6�  $	�4) ��6�  �� 6�  $@�:The device will load its virtual COM port (VCP). The port ��6�  �� 6  $�needs another ��6  �� 66  $<�6driver, which can be downloaded from Rohde & Schwarz��6:  �� 6|  $;�5 and is in the same package as the proprietary driver��6�  �� 6�  $�
 (step 2)��6�  �� 6�  $�.��6�  �� 6�  �� 6�  �� 6�  �� 6  $	�5) ��6  �� 6  $'�!If no COM port is visible in the ��6  �� 6C  $
��Bold"�6G  $��Italic$�6Y  $�Connection ��6l  �� 6�  $8�2menu in this application, check again whether the ��6�  �� 6�  $
��Bold"�6�  $��Italic$�6�  $�Load VCP��6�  �� 6�  $� is selected.��6 	  6	  �� 5   
   -   5         ��Name$ buttonDownload��5      $��Download driver��6)   $
��Left=�6C   $��
20,320,0,0q�6^   $	��Top=�6r   $	��150��6�   6�   �� 5   
   -   5        $ buttonDeviceManager��5      $��Open Device manager��6.   $
��Left=�6L   $��175,320,0,0q�6g   $	��Top=�6|   $	��150��6�   6�   �� 5   
   -   5        $ buttonClose��5      $��Close��6&   $
��Left=�66   $��447,320,0,0q�6Q   $	��Top=�6f   $��75��6~   6�   5      5      