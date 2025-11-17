-- Table: public.rooms

-- DROP TABLE IF EXISTS public.rooms;

CREATE TABLE IF NOT EXISTS public.rooms
(
    id uuid NOT NULL,
    name text COLLATE pg_catalog."default" NOT NULL,
    open_time time without time zone NOT NULL,
    close_time time without time zone NOT NULL,
    rows integer NOT NULL,
    cols integer NOT NULL,
    CONSTRAINT rooms_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.rooms
    OWNER to postgres;