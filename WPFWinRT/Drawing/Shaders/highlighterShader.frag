#version 330

//out vec4 outputColor;

in vec2 texCoord;

// A sampler2d is the representation of a texture in a shader.
// Each sampler is bound to a texture unit (texture units are described in Texture.cs on the Use function)
// By default, the unit is 0, so no code-related setup is actually needed.
// Multiple samplers will be demonstrated in section 1.5
uniform sampler2D texture0;

void main()
{
    // To use a texture, you call the texture() function.
    // It takes two parameters: the sampler to use, and a vec2, used as texture coordinates
    //vec4 _outputColor = texture(texture0, texCoord);
    //if(_outputColor.a == 0.0)
    //    discard;
    //outputColor = vec4(_outputColor.rgb, 0.1);

     vec4 outputColor = texture2D(texture0, texCoord);
     //outputColor.a *= 0.5;
   //  outputColor.g = 0.5;
     if(outputColor.a == 0.0)
        discard;
     gl_FragColor = outputColor;
}