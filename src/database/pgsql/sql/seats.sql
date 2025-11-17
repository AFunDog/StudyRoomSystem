-- Table: public.seats

-- DROP TABLE IF EXISTS public.seats;

CREATE TABLE IF NOT EXISTS public.seats
(
    id uuid NOT NULL,
    room_id uuid NOT NULL,
    "row" integer NOT NULL,
    col integer NOT NULL,
    CONSTRAINT seats_pkey PRIMARY KEY (id),
    CONSTRAINT "seats_roomId_fkey" FOREIGN KEY (room_id)
        REFERENCES public.rooms (id) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.seats
    OWNER to postgres;