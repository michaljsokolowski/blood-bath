Shader "Custom/AttackCone"
{
    Properties
    {
        [HDR] _ColorStart ("Color Start", Color) = (1.0, 1.0, 1.0, 1.0)
        [HDR] _ColorEnd ("Color End", Color) = (1.0, 1.0, 1.0, 1.0)
        [HDR] _ColorChargeStart ("Color Charge Start", Color) = (1.0, 1.0, 1.0, 1.0)
        [HDR] _ColorChargeEnd ("Color Charge End", Color) = (1.0, 1.0, 1.0, 1.0)
        
        _AttackRadius ("Attack Radius", Range(1.0, 10.0)) = 5.0
        _AttackAngle ("Attack Angle", Range(0.0, 360.0)) = 20.0
        _OutlineThickness ("Outline Thickness", Range(0.01, 1.0)) = 0.1
        _AttackCharge ("Attack Charge", Range(0.0, 1.0)) = 0.0
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "IgnoreProjector" = "True"
        }
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        BlendOp Add
        
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 localPos : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            // Passed from player script
            float4 playerForward;
            float4 playerPosition;
            
            float _AttackRadius;
            float _AttackAngle;
            float _OutlineThickness;
            float _AttackCharge;

            fixed4 _ColorStart;
            fixed4 _ColorEnd;
            fixed4 _ColorChargeStart;
            fixed4 _ColorChargeEnd;

            v2f vert (appdata v)
            {
                v2f o;

                o.localPos = v.vertex;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                const float3 playerToPoint     = (i.worldPos.xyz - playerPosition.xyz) * float3(1.0, 0.0, 1.0);
                const float3 playerToPointNorm = normalize(playerToPoint);

                const float distance           = length(playerToPoint);
                const float minRadius          = 0.7f;

                clip(_AttackRadius - distance);
                clip(distance - minRadius);
                
                const float angle = acos(dot(playerToPointNorm, playerForward.xyz)) * 180.0 * UNITY_INV_PI;
                _AttackAngle *= 0.5;

                clip(_AttackAngle - angle);

                const float charge = _AttackCharge * _AttackRadius;

                const float sdfCircleMin = abs(distance - minRadius);
                const float sdfCircleMax = abs(distance - _AttackRadius);
                const float sdfCharge    = abs(distance - charge);

                const float sinTheta = sin(radians(_AttackAngle));
                const float cosTheta = cos(radians(_AttackAngle));

                const float4x4 rotMatrixRight = float4x4(
                    cosTheta, 0.0, sinTheta, 0.0,
                    0.0, 1.0, 0.0, 0.0,
                    -sinTheta, 0.0, cosTheta, 0.0,
                    0.0, 0.0, 0.0, 1.0
                    );

                const float4x4 rotMatrixLeft = float4x4(
                    cosTheta, 0.0, -sinTheta, 0.0,
                    0.0, 1.0, 0.0, 0.0,
                    sinTheta, 0.0, cosTheta, 0.0,
                    0.0, 0.0, 0.0, 1.0
                    );
                
                const float3 lineRight     = mul(rotMatrixRight, playerForward * _AttackRadius);
                const float3 lineRightNorm = normalize(lineRight);
                const float3 projRight     = lineRightNorm * (dot(playerToPointNorm, lineRightNorm) * distance);

                const float3 lineLeft      = mul(rotMatrixLeft, playerForward * _AttackRadius);
                const float3 lineLeftNorm  = normalize(lineLeft);
                const float3 projLeft      = lineLeftNorm * (dot(playerToPointNorm, lineLeftNorm) * distance);
                
                const float distLineLeft   = length(playerToPoint - projLeft);
                const float distLineRight  = length(playerToPoint - projRight);

                const bool isSameHalfSpaceRight = acos(dot(playerToPointNorm, lineRightNorm)) < UNITY_HALF_PI;
                const bool isSameHalfSpaceLeft  = acos(dot(playerToPointNorm, lineLeftNorm)) < UNITY_HALF_PI;

                const bool outline = sdfCircleMin < _OutlineThickness || sdfCircleMax < _OutlineThickness
                        || (distLineRight < _OutlineThickness && isSameHalfSpaceRight) || (distLineLeft < _OutlineThickness && isSameHalfSpaceLeft)
                        || sdfCharge < _OutlineThickness * 0.5;

                const float interp = clamp(distance / _AttackRadius, 0.0, 1.0);
                fixed4 col = fixed4((interp * _ColorEnd + (1.0 - interp) * _ColorStart).xyz, 0.2);
                
                if (outline)
                {
                    col.a = 0.9;
                }
                else if (distance < charge)
                {
                    col = interp * _ColorChargeEnd + (1.0 - interp) * _ColorChargeStart;
                }

                return col;
            }
            ENDCG
        }
    }
}
