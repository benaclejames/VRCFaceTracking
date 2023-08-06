namespace VRCFaceTracking.Core.OSC.Query.mDNS;

public class DNSPacket
{
    public ushort ID;
    public bool QUERYRESPONSE;
    public int OPCODE;
    public bool CONFLICT;
    public bool TRUNCATION;
    public bool TENTATIVE;
    public int RESPONSECODE;
    public DNSQuestion[] questions;
    public DNSResource[] answers;
    public DNSResource[] authorities;
    public DNSResource[] additionals;
        

    public DNSPacket(BigReader stream)
    {
        /**
            Bit offset	0	1	2	3	4	5	6	7	8	9	10	11	12	13	14	15
            0	ID
            16	QR	Opcode	C	TC	T	Z	Z	Z	Z	RCODE
            32	QDCOUNT
            48	ANCOUNT
            64	NSCOUNT
            80	ARCOUNT
         */
            
        // Read the header
        ID = stream.ReadUInt16();

        var flags = stream.ReadUInt16();
        QUERYRESPONSE = (flags & 0x8000) == 0x8000;
        OPCODE = (flags & 0x7800) >> 11;
        CONFLICT = (flags & 0x0400) == 0x0400;
        TRUNCATION = (flags & 0x0200) == 0x0200;
        TENTATIVE = (flags & 0x0100) == 0x0100;
        RESPONSECODE = flags & 0x000F;

        questions = new DNSQuestion[stream.ReadUInt16()];
        answers = new DNSResource[stream.ReadUInt16()];
        authorities = new DNSResource[stream.ReadUInt16()];
        additionals = new DNSResource[stream.ReadUInt16()];
            
        for (int i = 0; i < questions.Length; i++)
            questions[i] = new DNSQuestion(stream);

        for (int i = 0; i < answers.Length; i++)
            answers[i] = new DNSResource(stream);
            
        for (int i = 0; i < authorities.Length; i++)
            authorities[i] = new DNSResource(stream);
            
        for (int i = 0; i < additionals.Length; i++)
            additionals[i] = new DNSResource(stream);
    }

    public DNSPacket()
    {
        QUERYRESPONSE = true;
    }

    public byte[] Serialize()
    {
        List<byte> bytes = new List<byte>();
            
        bytes.AddRange(BigWriter.WriteUInt16(ID));
            
        ushort flags = 0;
        if (QUERYRESPONSE)
            flags |= 0x8000;
        if (CONFLICT)
            flags |= 0x0400;
        if (TRUNCATION)
            flags |= 0x0200;
            
        flags |= (ushort)(OPCODE << 11);
        flags |= (ushort)(RESPONSECODE & 0x000F);
            
        bytes.AddRange(BigWriter.WriteUInt16(flags));
            
        bytes.AddRange(BigWriter.WriteUInt16((ushort)questions.Length));
            
        bytes.AddRange(BigWriter.WriteUInt16((ushort)answers.Length));
            
        bytes.AddRange(BigWriter.WriteUInt16((ushort)authorities.Length));
            
        bytes.AddRange(BigWriter.WriteUInt16((ushort)additionals.Length));
            
        foreach (var question in questions)
            bytes.AddRange(question.Serialize());
            
        foreach (var answer in answers)
            bytes.AddRange(answer.Serialize());
            
        foreach (var authority in authorities)
            bytes.AddRange(authority.Serialize());
            
        foreach (var additional in additionals)
            bytes.AddRange(additional.Serialize());
            
        return bytes.ToArray();
    }
}