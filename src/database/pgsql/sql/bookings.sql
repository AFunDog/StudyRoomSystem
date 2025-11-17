-- Table: public.bookings

-- DROP TABLE IF EXISTS public.bookings;

CREATE TABLE IF NOT EXISTS public.bookings
(
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    seat_id uuid NOT NULL,
    create_time timestamp with time zone NOT NULL,
    start_time timestamp with time zone NOT NULL,
    end_time timestamp with time zone NOT NULL,
    check_in_time timestamp with time zone,
    check_out_time timestamp with time zone,
    state text COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT bookings_pkey PRIMARY KEY (id),
    CONSTRAINT bookings_seat_id_fkey FOREIGN KEY (seat_id)
        REFERENCES public.seats (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT bookings_user_id_fkey FOREIGN KEY (user_id)
        REFERENCES public.users (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.bookings
    OWNER to postgres;

-- Trigger: on_data_change

-- DROP TRIGGER IF EXISTS on_data_change ON public.bookings;

CREATE OR REPLACE TRIGGER on_data_change
    AFTER INSERT OR DELETE OR UPDATE 
    ON public.bookings
    FOR EACH ROW
    EXECUTE FUNCTION public.notify_data_change();