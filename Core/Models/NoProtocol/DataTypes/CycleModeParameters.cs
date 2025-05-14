namespace Core.Models.NoProtocol.DataTypes;

public class CycleModeParameters
{
    public readonly byte[] MessageBytes;

    public readonly bool Response_Date_Enable = false;
    public readonly bool Response_Time_Enable = false;

    public readonly bool Response_String_Start_Enable = false;
    public readonly string? Response_String_Start;

    public readonly bool Response_String_End_Enable = false;
    public readonly string? Response_String_End;

    public readonly bool Response_NextLine_Enable = false;

    public CycleModeParameters(
        byte[] messageBytes,
        bool response_Date_Enable,
        bool response_Time_Enable,
        bool response_String_Start_Enable,
        string? response_String_Start,
        bool response_String_End_Enable,
        string? response_String_End,
        bool response_NextLine_Enable)
    {
        MessageBytes = messageBytes;
        Response_Date_Enable = response_Date_Enable;
        Response_Time_Enable = response_Time_Enable;
        Response_String_Start_Enable = response_String_Start_Enable;
        Response_String_Start = response_String_Start;
        Response_String_End_Enable = response_String_End_Enable;
        Response_String_End = response_String_End;
        Response_NextLine_Enable = response_NextLine_Enable;
    }
}
