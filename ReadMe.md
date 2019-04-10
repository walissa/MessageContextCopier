This component copies the message context properties to the received message in BizTalk so they can be retrieved and used in BizTalk Maps.
<br>
Because of the component's behavior so it should be used at the receiving side, and it is recommended to be the last component in the receive pipeline.
<br>
The properties and their info would be available as end of the message before closing the root node.
<br>
The message context would appear in one of the following formats based on the component's settings:
```sh
<messagecontext>
    <property name="prop1" namespace="http://somenamespace" promoted="false" >some value</property>
</messagecontext>
-- or --
<messagecontext>
    <property>
      <name>prop1</name>
      <namespace>http://somenamespace</namespace>
      <promoted>False</promoted>
      <value>some value</value>
    </property>
</messagecontext>
```
In the map, the properties can be retrieved using the following C# function:
<br>
```sh
<msxsl:script language="C#" implements-prefix="CSharp">
<msxsl:assembly name="System.Globalization"/>
<msxsl:using namespace="System.Xml"/>
<![CDATA[
    public object ConvertToNode(string value)
    {
        if(string.IsNullOrEmpty(value))
            value = "<messagecontext />";
        var doc = new System.Xml.XmlDocument();                        
        doc.LoadXml(value);
        var nav = doc.CreateNavigator();
        var ret = nav.Select("messagecontext");
        return ret;
    }
]]>
</msxsl:script>
```
```sh
<xsl:variable name="msgctx" select="CSharp:ConvertToNode(//comment()[starts-with(.,'&lt;messagecontext&gt;')])"/>

<xsl:value-of select="$msgctx/property[@name='prop1' and @namespace='http://somenamespace']/text()"/>
or 
<xsl:value-of select="$msgctx/property[name/text()='prop1' and namespace/text()='http://somenamespace']/text()"/>
```
<br>
<br>

