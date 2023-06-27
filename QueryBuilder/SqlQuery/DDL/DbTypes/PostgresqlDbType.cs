namespace SqlKata.SqlQuery.DDL.DbTypes
{
    public enum PostgresqlDbType
    {
        Smallint,
        Integer,
        Bigint,
        Smallserial,
        Serial,
        Bigserial,

        Boolean,

        Numeric,
        Double_precision,
        Real,

        Timestamp_with_time_zone,
        Timestamp_without_time_zone,
        Date,
        Time_with_time_zone,
        Time_without_time_zone,

        Character,
        Character_varying,
        Char,

        Text,

        Uuid,


        //not supported types
        Bytea,
        Bit,
        Bit_varying,
        Interval,
        Box,
        Cidr,
        Circle,
        Inet,
        Json,
        Jsonb,
        Line,
        Lseg,
        Macaddr,
        Money,
        Path,
        Pg_ln,
        Point,
        Polygon,
        Tsquery,
        Tsvector,
        Txid_snapshot,
        Xml
    }
}
