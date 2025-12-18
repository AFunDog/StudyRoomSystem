--
-- PostgreSQL database dump
--

\restrict Y8gbW4Z1tel80SuJGmVL2cNMahMdU8EzGoA1TuGy2a5KK1qGH2XOVTZSCmNheEV

-- Dumped from database version 18.1 (Ubuntu 18.1-1.pgdg22.04+2)
-- Dumped by pg_dump version 18.1 (Ubuntu 18.1-1.pgdg22.04+2)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: audit_log(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.audit_log() RETURNS trigger
    LANGUAGE plpgsql
    AS $$BEGIN
    -- 插入操作：记录 new_data
    IF (TG_OP = 'INSERT') THEN
        INSERT INTO public.audit_logs (
            table_name, row_id, operation, new_data
        ) VALUES (
            TG_TABLE_NAME, NEW.id, 'I', to_jsonb(NEW)
        );
        RETURN NEW;
    -- 更新操作：记录 old_data + new_data
    ELSIF (TG_OP = 'UPDATE') THEN
        INSERT INTO public.audit_logs (
            table_name, row_id, operation, old_data, new_data
        ) VALUES (
            TG_TABLE_NAME, NEW.id, 'U', to_jsonb(OLD), to_jsonb(NEW)
        );
        RETURN NEW;
    -- 删除操作：记录 old_data
    ELSIF (TG_OP = 'DELETE') THEN
        INSERT INTO public.audit_logs (
            table_name, row_id, operation, old_data
        ) VALUES (
            TG_TABLE_NAME, OLD.id, 'D', to_jsonb(OLD)
        );
        RETURN OLD;
    END IF;
END;$$;


ALTER FUNCTION public.audit_log() OWNER TO postgres;

--
-- Name: notify_data_change(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.notify_data_change() RETURNS trigger
    LANGUAGE plpgsql
    AS $$DECLARE	
	payload TEXT;
BEGIN

	payload := json_build_object(
		'table',TG_TABLE_NAME,
		'operation',TG_OP,
		'dataId',COALESCE(NEW.id,OLD.id)
	)::text;

	PERFORM pg_notify('data_change',payload);
	RETURN NEW;
END$$;


ALTER FUNCTION public.notify_data_change() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: audit_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.audit_logs (
    id uuid DEFAULT uuidv7() NOT NULL,
    table_name text NOT NULL,
    row_id uuid NOT NULL,
    operation character(1) NOT NULL,
    old_data jsonb,
    new_data jsonb,
    operate_user text DEFAULT CURRENT_USER NOT NULL,
    operate_time timestamp with time zone DEFAULT now()
);


ALTER TABLE public.audit_logs OWNER TO postgres;

--
-- Name: blacklists; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.blacklists (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    type text NOT NULL,
    reason text NOT NULL,
    create_time timestamp with time zone NOT NULL,
    expire_time timestamp with time zone NOT NULL
);


ALTER TABLE public.blacklists OWNER TO postgres;

--
-- Name: bookings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.bookings (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    seat_id uuid NOT NULL,
    create_time timestamp with time zone NOT NULL,
    start_time timestamp with time zone NOT NULL,
    end_time timestamp with time zone NOT NULL,
    check_in_time timestamp with time zone,
    check_out_time timestamp with time zone,
    state text NOT NULL
);


ALTER TABLE public.bookings OWNER TO postgres;

--
-- Name: complaints; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.complaints (
    id uuid NOT NULL,
    send_user_id uuid NOT NULL,
    state text NOT NULL,
    type text NOT NULL,
    send_content text CONSTRAINT complaints_content_not_null NOT NULL,
    create_time timestamp with time zone NOT NULL,
    handle_time timestamp with time zone,
    handle_user_id uuid,
    seat_id uuid NOT NULL,
    handle_content text,
    target_time timestamp with time zone
);


ALTER TABLE public.complaints OWNER TO postgres;

--
-- Name: rooms; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.rooms (
    id uuid NOT NULL,
    name text NOT NULL,
    open_time time without time zone NOT NULL,
    close_time time without time zone NOT NULL,
    rows integer NOT NULL,
    cols integer NOT NULL
);


ALTER TABLE public.rooms OWNER TO postgres;

--
-- Name: seats; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.seats (
    id uuid NOT NULL,
    room_id uuid NOT NULL,
    "row" integer NOT NULL,
    col integer NOT NULL
);


ALTER TABLE public.seats OWNER TO postgres;

--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id uuid NOT NULL,
    create_time timestamp with time zone NOT NULL,
    user_name text NOT NULL,
    display_name text NOT NULL,
    password text NOT NULL,
    campus_id text NOT NULL,
    phone text NOT NULL,
    email text,
    role text NOT NULL,
    avatar text
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: violations; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.violations (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    state text NOT NULL,
    type text NOT NULL,
    content text NOT NULL,
    booking_id uuid NOT NULL,
    create_time timestamp with time zone NOT NULL
);


ALTER TABLE public.violations OWNER TO postgres;

--
-- Data for Name: audit_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.audit_logs (id, table_name, row_id, operation, old_data, new_data, operate_user, operate_time) FROM stdin;
019b2f1f-853d-764f-9933-197ec074d555	bookings	019afbd2-28db-c438-69bd-021b3fd06b63	U	{"id": "019afbd2-28db-c438-69bd-021b3fd06b63", "state": "Booked", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019a1b60-de17-ead2-3902-26fb51efc5bf", "end_time": "2025-12-08T23:14:14.873824+08:00", "start_time": "2025-12-08T22:14:14.873824+08:00", "create_time": "2025-12-08T10:37:25.60505+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019afbd2-28db-c438-69bd-021b3fd06b63", "state": "Canceled", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019a1b60-de17-ead2-3902-26fb51efc5bf", "end_time": "2025-12-08T23:14:14.873824+08:00", "start_time": "2025-12-08T22:14:14.873824+08:00", "create_time": "2025-12-08T10:37:25.60505+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f1f-853e-70fc-b7b8-ee7ad5052af1	bookings	019afbd6-8723-d5e2-11ff-9ff5fab254c2	U	{"id": "019afbd6-8723-d5e2-11ff-9ff5fab254c2", "state": "Booked", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019a1b60-de17-ead2-3902-26fb51efc5bf", "end_time": "2025-12-08T21:22:54.336911+08:00", "start_time": "2025-12-08T20:22:54.336911+08:00", "create_time": "2025-12-08T10:42:11.882352+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019afbd6-8723-d5e2-11ff-9ff5fab254c2", "state": "Canceled", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019a1b60-de17-ead2-3902-26fb51efc5bf", "end_time": "2025-12-08T21:22:54.336911+08:00", "start_time": "2025-12-08T20:22:54.336911+08:00", "create_time": "2025-12-08T10:42:11.882352+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f1f-853e-724f-825c-1dd560b336e1	bookings	019b07ea-968c-4138-4d88-18066a779be8	U	{"id": "019b07ea-968c-4138-4d88-18066a779be8", "state": "Booked", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-11T18:58:22.303866+08:00", "start_time": "2025-12-11T16:58:22.303866+08:00", "create_time": "2025-12-10T18:59:33.13445+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b07ea-968c-4138-4d88-18066a779be8", "state": "Canceled", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-11T18:58:22.303866+08:00", "start_time": "2025-12-11T16:58:22.303866+08:00", "create_time": "2025-12-10T18:59:33.13445+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f1f-853e-7332-8c7c-85cb180038d8	bookings	019b0b75-fe6e-9ced-0d10-ae2a5f24a1a6	U	{"id": "019b0b75-fe6e-9ced-0d10-ae2a5f24a1a6", "state": "Booked", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-10T12:55:00+08:00", "start_time": "2025-12-10T10:40:00+08:00", "create_time": "2025-12-11T11:30:40.878246+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b0b75-fe6e-9ced-0d10-ae2a5f24a1a6", "state": "Canceled", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-10T12:55:00+08:00", "start_time": "2025-12-10T10:40:00+08:00", "create_time": "2025-12-11T11:30:40.878246+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f1f-853e-73ff-b8d2-d828d1376736	bookings	019b0c15-8355-5686-007e-8be225bf6d9a	U	{"id": "019b0c15-8355-5686-007e-8be225bf6d9a", "state": "Booked", "seat_id": "019b0bf4-0caa-f408-cd73-902810499248", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-11T14:40:00+08:00", "start_time": "2025-12-11T12:40:00+08:00", "create_time": "2025-12-11T14:24:55.12519+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b0c15-8355-5686-007e-8be225bf6d9a", "state": "Canceled", "seat_id": "019b0bf4-0caa-f408-cd73-902810499248", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-11T14:40:00+08:00", "start_time": "2025-12-11T12:40:00+08:00", "create_time": "2025-12-11T14:24:55.12519+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f1f-853e-74cd-861b-3cac884b57b3	bookings	019b0c27-fda8-67cc-187b-27ef29c6b804	U	{"id": "019b0c27-fda8-67cc-187b-27ef29c6b804", "state": "Booked", "seat_id": "019b0bf4-03f0-03e6-7063-73e2354db38a", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-01T11:45:00+08:00", "start_time": "2025-12-01T08:40:00+08:00", "create_time": "2025-12-11T14:45:06.08866+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b0c27-fda8-67cc-187b-27ef29c6b804", "state": "Canceled", "seat_id": "019b0bf4-03f0-03e6-7063-73e2354db38a", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-01T11:45:00+08:00", "start_time": "2025-12-01T08:40:00+08:00", "create_time": "2025-12-11T14:45:06.08866+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f1f-853e-755a-a5c1-29656c430f0f	bookings	019b0c67-0984-9ca7-a8d3-1236c9f61e19	U	{"id": "019b0c67-0984-9ca7-a8d3-1236c9f61e19", "state": "Booked", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-01T13:50:00+08:00", "start_time": "2025-12-01T10:40:00+08:00", "create_time": "2025-12-11T15:53:57.892685+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b0c67-0984-9ca7-a8d3-1236c9f61e19", "state": "Canceled", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-01T13:50:00+08:00", "start_time": "2025-12-01T10:40:00+08:00", "create_time": "2025-12-11T15:53:57.892685+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f1f-853e-75e3-8b2a-f202056e954c	bookings	019b0c6a-5e86-26d4-4544-29e5e89f2acf	U	{"id": "019b0c6a-5e86-26d4-4544-29e5e89f2acf", "state": "Booked", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-12T11:00:00+08:00", "start_time": "2025-12-12T10:00:00+08:00", "create_time": "2025-12-11T15:57:36.262531+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b0c6a-5e86-26d4-4544-29e5e89f2acf", "state": "Canceled", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-12T11:00:00+08:00", "start_time": "2025-12-12T10:00:00+08:00", "create_time": "2025-12-11T15:57:36.262531+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f1f-853e-7694-af8b-2cef52f7803a	bookings	019b0c6c-0288-6927-2048-734a88ae12e1	U	{"id": "019b0c6c-0288-6927-2048-734a88ae12e1", "state": "Booked", "seat_id": "019b0c31-bfc6-f030-5446-63149a76d12f", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-11T13:00:00+08:00", "start_time": "2025-12-11T12:00:00+08:00", "create_time": "2025-12-11T15:59:23.78425+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b0c6c-0288-6927-2048-734a88ae12e1", "state": "Canceled", "seat_id": "019b0c31-bfc6-f030-5446-63149a76d12f", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-11T13:00:00+08:00", "start_time": "2025-12-11T12:00:00+08:00", "create_time": "2025-12-11T15:59:23.78425+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f1f-853e-7726-a4d1-50a5eb5f9741	bookings	019b0c88-8f5a-e86d-a4ea-851458b2db98	U	{"id": "019b0c88-8f5a-e86d-a4ea-851458b2db98", "state": "Booked", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019b0818-6889-9081-34a3-d1da36233462", "end_time": "2025-12-12T10:05:00+08:00", "start_time": "2025-12-12T10:00:00+08:00", "create_time": "2025-12-11T16:30:34.842854+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b0c88-8f5a-e86d-a4ea-851458b2db98", "state": "Canceled", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019b0818-6889-9081-34a3-d1da36233462", "end_time": "2025-12-12T10:05:00+08:00", "start_time": "2025-12-12T10:00:00+08:00", "create_time": "2025-12-11T16:30:34.842854+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f1f-853e-779c-8d64-4cf0b76e3e1d	bookings	019b0c8f-20f1-2e0c-2321-b56a3ba682ca	U	{"id": "019b0c8f-20f1-2e0c-2321-b56a3ba682ca", "state": "Booked", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-12T15:00:00+08:00", "start_time": "2025-12-12T13:00:00+08:00", "create_time": "2025-12-11T16:37:45.329849+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b0c8f-20f1-2e0c-2321-b56a3ba682ca", "state": "Canceled", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-12T15:00:00+08:00", "start_time": "2025-12-12T13:00:00+08:00", "create_time": "2025-12-11T16:37:45.329849+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f1f-853e-7b4b-91e1-80caecfa2fd0	bookings	019b0d5b-e385-5bb3-5414-9834ae6557a1	U	{"id": "019b0d5b-e385-5bb3-5414-9834ae6557a1", "state": "Booked", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b0818-6889-9081-34a3-d1da36233462", "end_time": "2025-12-13T12:00:00+08:00", "start_time": "2025-12-13T10:00:00+08:00", "create_time": "2025-12-11T20:21:24.489061+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b0d5b-e385-5bb3-5414-9834ae6557a1", "state": "Canceled", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b0818-6889-9081-34a3-d1da36233462", "end_time": "2025-12-13T12:00:00+08:00", "start_time": "2025-12-13T10:00:00+08:00", "create_time": "2025-12-11T20:21:24.489061+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f1f-853e-7bde-b32b-810ca865922b	bookings	019b2180-93c2-831b-6e94-54538bfbf665	U	{"id": "019b2180-93c2-831b-6e94-54538bfbf665", "state": "Booked", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-16T12:00:00+08:00", "start_time": "2025-12-16T11:00:00+08:00", "create_time": "2025-12-15T18:13:53.229478+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2180-93c2-831b-6e94-54538bfbf665", "state": "Canceled", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-16T12:00:00+08:00", "start_time": "2025-12-16T11:00:00+08:00", "create_time": "2025-12-15T18:13:53.229478+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:42:33.532735+08
019b2f21-d73b-7bd9-b628-347cc2c7d6a3	bookings	019a4cd3-4750-e38a-9e5d-8f824d042851	D	{"id": "019a4cd3-4750-e38a-9e5d-8f824d042851", "state": "Canceled", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019a1b60-de17-ead2-3902-26fb51efc5bf", "end_time": "2025-11-05T05:40:00+08:00", "start_time": "2025-11-05T04:25:00+08:00", "create_time": "2025-11-04T11:05:06.133175+08:00", "check_in_time": null, "check_out_time": null}	\N	postgres	2025-12-18 09:45:05.513306+08
019b2f21-d73c-731b-b1fc-487f4aa0abcb	bookings	019a4cd3-4750-e38a-9e5d-8f824d042852	D	{"id": "019a4cd3-4750-e38a-9e5d-8f824d042852", "state": "Canceled", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019a1b60-de17-ead2-3902-26fb51efc5bf", "end_time": "2025-11-05T05:40:00+08:00", "start_time": "2025-11-05T04:25:00+08:00", "create_time": "2025-11-04T11:05:06.133175+08:00", "check_in_time": null, "check_out_time": null}	\N	postgres	2025-12-18 09:45:05.513306+08
019b2f21-d73c-7542-8fc6-d5b1b838123e	bookings	019a7ba6-0a1a-ccd3-56d1-e7f6b4da47f0	D	{"id": "019a7ba6-0a1a-ccd3-56d1-e7f6b4da47f0", "state": "Canceled", "seat_id": "019a1e1b-2c7a-fcdb-6ebe-8e70904583a8", "user_id": "019a1b60-de17-ead2-3902-26fb51efc5bf", "end_time": "2025-11-19T11:40:00+08:00", "start_time": "2025-11-19T06:30:00+08:00", "create_time": "2025-11-13T13:17:50.496151+08:00", "check_in_time": null, "check_out_time": null}	\N	postgres	2025-12-18 09:45:05.513306+08
019b2f2a-7ca2-773f-8d7b-c7de2005d1e8	bookings	019b2f2a-703b-5d87-c96f-be081a2a7b4d	I	\N	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Booked", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-19T11:00:00+08:00", "start_time": "2025-12-19T10:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:54:32.223945+08
019b2f2c-9c2e-7252-9a63-fdad2041070e	bookings	019b2f2c-9214-0952-a297-f649f9b0b87f	I	\N	{"id": "019b2f2c-9214-0952-a297-f649f9b0b87f", "state": "Booked", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T11:00:00+08:00", "start_time": "2025-12-18T10:00:00+08:00", "create_time": "2025-12-18T09:56:48.788464+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:56:51.373653+08
019b2f2c-b5dc-78b2-ab8a-1a1d91724c75	bookings	019b2f2c-9214-0952-a297-f649f9b0b87f	U	{"id": "019b2f2c-9214-0952-a297-f649f9b0b87f", "state": "Booked", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T11:00:00+08:00", "start_time": "2025-12-18T10:00:00+08:00", "create_time": "2025-12-18T09:56:48.788464+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2c-9214-0952-a297-f649f9b0b87f", "state": "CheckIn", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T11:00:00+08:00", "start_time": "2025-12-18T10:00:00+08:00", "create_time": "2025-12-18T09:56:48.788464+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:56:57.948068+08
019b2f2d-c7ef-7a90-801f-9b3b1a363ccb	bookings	019b2f2a-703b-5d87-c96f-be081a2a7b4d	U	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Booked", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-19T11:00:00+08:00", "start_time": "2025-12-19T10:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Booked", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:58:08.044902+08
019b2f2e-b139-7c52-8d3c-0c71c20efc6b	bookings	019b2f2a-703b-5d87-c96f-be081a2a7b4d	U	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Booked", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Canceled", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 09:59:07.830885+08
019b2f32-875e-773b-b293-b9b65cb5979e	bookings	019b1d26-c56f-c648-536b-cd5c1e79d7fc	U	{"id": "019b1d26-c56f-c648-536b-cd5c1e79d7fc", "state": "Canceled", "seat_id": "019b0bf4-03f0-03e6-7063-73e2354db38a", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-31T20:00:00+08:00", "start_time": "2025-12-31T08:00:00+08:00", "create_time": "2025-12-14T21:57:18.831877+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b1d26-c56f-c648-536b-cd5c1e79d7fc", "state": "Booked", "seat_id": "019b0bf4-03f0-03e6-7063-73e2354db38a", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-31T20:00:00+08:00", "start_time": "2025-12-31T08:00:00+08:00", "create_time": "2025-12-14T21:57:18.831877+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 10:03:19.229902+08
019b2f39-c525-7a8d-a902-2965f0ec30ef	bookings	019b2f2a-703b-5d87-c96f-be081a2a7b4d	U	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Canceled", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Booked", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 10:11:13.788336+08
019b2f39-c92b-7ec9-ba9a-2f474acaa190	bookings	019b2f2a-703b-5d87-c96f-be081a2a7b4d	U	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Booked", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Canceled", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 10:11:14.858321+08
019b2f3a-1c9f-78cc-a3e4-897b2afd2565	bookings	019b2f2a-703b-5d87-c96f-be081a2a7b4d	U	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Canceled", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Cancel", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 10:11:36.183201+08
019b2f3b-5be9-7b22-96db-f878d02ac401	bookings	019b2f2a-703b-5d87-c96f-be081a2a7b4d	U	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Cancel", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Booked", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 10:12:57.919505+08
019b2f3b-aa0d-7068-affa-6df480eb67f3	bookings	019b2f2a-703b-5d87-c96f-be081a2a7b4d	U	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Booked", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Canceled", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 10:13:17.963434+08
019b2f3c-a31b-7498-8bf8-b3fb7ed5ce80	bookings	019b2f2a-703b-5d87-c96f-be081a2a7b4d	U	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Canceled", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Booked", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 10:14:21.683764+08
019b2f3d-125b-7619-bcc7-2dc33ad54d12	bookings	019b2f2a-703b-5d87-c96f-be081a2a7b4d	U	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Booked", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Canceled", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 10:14:50.200778+08
019b2f3e-da49-7487-a057-8f4409004f28	bookings	019b2f2a-703b-5d87-c96f-be081a2a7b4d	U	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Canceled", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Booked", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 10:16:46.869913+08
019b2f3f-0d7f-7bbd-8a96-de481ed8b10f	bookings	019b2f2a-703b-5d87-c96f-be081a2a7b4d	U	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Booked", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2a-703b-5d87-c96f-be081a2a7b4d", "state": "Canceled", "seat_id": "019b0bf4-0555-117d-eb34-e0d634c38793", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T09:30:00+08:00", "start_time": "2025-12-18T09:00:00+08:00", "create_time": "2025-12-18T09:54:29.062756+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 10:17:00.030057+08
019b2fea-eed3-7e2c-ad1d-3c12b758755b	bookings	019b2f2c-9214-0952-a297-f649f9b0b87f	U	{"id": "019b2f2c-9214-0952-a297-f649f9b0b87f", "state": "CheckIn", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T11:00:00+08:00", "start_time": "2025-12-18T10:00:00+08:00", "create_time": "2025-12-18T09:56:48.788464+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b2f2c-9214-0952-a297-f649f9b0b87f", "state": "Canceled", "seat_id": "019b0bf4-036e-0e48-a96a-7277084ec5a2", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T11:00:00+08:00", "start_time": "2025-12-18T10:00:00+08:00", "create_time": "2025-12-18T09:56:48.788464+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 13:24:44.370287+08
019b2fea-f013-7595-a526-b6f6bf784f0e	violations	019b2fea-ee77-f0e0-3ca0-051a67c9c5b1	I	\N	{"id": "019b2fea-ee77-f0e0-3ca0-051a67c9c5b1", "type": "超时", "state": "Violation", "content": "未在规定时间签退", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "booking_id": "019b2f2c-9214-0952-a297-f649f9b0b87f", "create_time": "2025-12-18T13:24:44.28161+08:00"}	postgres	2025-12-18 13:24:44.690156+08
019b2ff0-78f6-7576-895a-3f540793d89e	users	019b2ff0-76c0-be59-fdd2-678ca2438a22	I	\N	{"id": "019b2ff0-76c0-be59-fdd2-678ca2438a22", "role": "User", "email": null, "phone": "17843570218", "avatar": null, "password": "$2a$11$3cybhTLAcj10UU9Krys3EulyP1CTbr6eLooqikqFApGNh8J3jYu0C", "campus_id": "2140533921", "user_name": "testuser09999", "create_time": "2025-12-18T13:30:46.848706+08:00", "display_name": "testuser09999"}	postgres	2025-12-18 13:30:47.412924+08
019b2ff3-0385-7b52-8ea9-418855ed671d	complaints	019b2ff2-f765-c3c4-91f3-d717f45b3d82	I	\N	{"id": "019b2ff2-f765-c3c4-91f3-d717f45b3d82", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:33:30.85912+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	postgres	2025-12-18 13:33:33.955647+08
019b2ff4-c39f-79c1-99f0-d615284c614b	complaints	019b2ff4-b730-49bf-c465-7b13410fb99f	I	\N	{"id": "019b2ff4-b730-49bf-c465-7b13410fb99f", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:35:25.492102+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	postgres	2025-12-18 13:35:28.668691+08
019b2ff7-2253-7678-9476-5c03f6d5e2eb	complaints	019b2ff7-17fb-119b-986b-22faabd2fe05	I	\N	{"id": "019b2ff7-17fb-119b-986b-22faabd2fe05", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:38:01.339042+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	postgres	2025-12-18 13:38:03.986865+08
019b2ff8-b593-785e-ba4a-fe44fb784e87	complaints	019b2ff8-ab39-617a-c4c7-31c79d9eafc9	I	\N	{"id": "019b2ff8-ab39-617a-c4c7-31c79d9eafc9", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:39:44.569751+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	postgres	2025-12-18 13:39:47.219025+08
019b2ff9-3110-71ee-a203-1b52cb14b1dd	complaints	019b2ff9-2394-b0ae-4bba-cb880ca05f9e	I	\N	{"id": "019b2ff9-2394-b0ae-4bba-cb880ca05f9e", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:40:15.384366+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	postgres	2025-12-18 13:40:18.829795+08
019b2ff9-ec18-7a22-9dfb-4e099b5f8b10	complaints	019b2ff9-deac-f845-a462-4fc6ad618caf	I	\N	{"id": "019b2ff9-deac-f845-a462-4fc6ad618caf", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:41:03.280745+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	postgres	2025-12-18 13:41:06.710289+08
019b2ffa-3177-788a-ad3c-732484d8e008	users	019b2ffa-2f24-56d0-ab5b-b1baea46b499	I	\N	{"id": "019b2ffa-2f24-56d0-ab5b-b1baea46b499", "role": "User", "email": null, "phone": "17845980218", "avatar": null, "password": "$2a$11$isNbD2oBqNAZybOa2zZNt.V1c4FP0F4BUJGDAlC8K/IOiEATJJ4Ou", "campus_id": "257003921", "user_name": "A_鞠俊材", "create_time": "2025-12-18T13:41:23.876239+08:00", "display_name": "A_鞠俊材"}	postgres	2025-12-18 13:41:24.467684+08
019b2ffa-5f66-7680-adcf-dae7e0670617	complaints	019b2ffa-54c4-bf90-58cb-b2caae728057	I	\N	{"id": "019b2ffa-54c4-bf90-58cb-b2caae728057", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:41:33.50868+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	postgres	2025-12-18 13:41:36.229873+08
019b2ffb-e79b-76ee-8f56-bdae9cbc8b52	complaints	019b2ffb-da3f-54fe-3e74-8eefd125c2b0	I	\N	{"id": "019b2ffb-da3f-54fe-3e74-8eefd125c2b0", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:43:13.219231+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	postgres	2025-12-18 13:43:16.633145+08
019b2ffc-7994-7c2d-a27a-846914592585	complaints	019b2ff2-f765-c3c4-91f3-d717f45b3d82	D	{"id": "019b2ff2-f765-c3c4-91f3-d717f45b3d82", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:33:30.85912+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	\N	postgres	2025-12-18 13:43:53.798467+08
019b2ffc-7994-7ebd-aae8-2cf33b45bd5e	complaints	019b2ff4-b730-49bf-c465-7b13410fb99f	D	{"id": "019b2ff4-b730-49bf-c465-7b13410fb99f", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:35:25.492102+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	\N	postgres	2025-12-18 13:43:53.798467+08
019b2ffc-7994-7f76-b971-101205591068	complaints	019b2ff7-17fb-119b-986b-22faabd2fe05	D	{"id": "019b2ff7-17fb-119b-986b-22faabd2fe05", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:38:01.339042+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	\N	postgres	2025-12-18 13:43:53.798467+08
019b2ffc-7995-7008-89c2-32d0e3300719	complaints	019b2ff8-ab39-617a-c4c7-31c79d9eafc9	D	{"id": "019b2ff8-ab39-617a-c4c7-31c79d9eafc9", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:39:44.569751+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	\N	postgres	2025-12-18 13:43:53.798467+08
019b2ffc-7995-70a2-8d45-ac1f18589122	complaints	019b2ff9-2394-b0ae-4bba-cb880ca05f9e	D	{"id": "019b2ff9-2394-b0ae-4bba-cb880ca05f9e", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:40:15.384366+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	\N	postgres	2025-12-18 13:43:53.798467+08
019b2ffc-7995-7130-9024-3509bde30958	complaints	019b2ff9-deac-f845-a462-4fc6ad618caf	D	{"id": "019b2ff9-deac-f845-a462-4fc6ad618caf", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:41:03.280745+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	\N	postgres	2025-12-18 13:43:53.798467+08
019b2ffc-7995-718c-ba89-22aa500c3063	complaints	019b2ffa-54c4-bf90-58cb-b2caae728057	D	{"id": "019b2ffa-54c4-bf90-58cb-b2caae728057", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:41:33.50868+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	\N	postgres	2025-12-18 13:43:53.798467+08
019b2ffd-3d29-7d84-81ed-a7f7df082929	complaints	019b2ffb-da3f-54fe-3e74-8eefd125c2b0	D	{"id": "019b2ffb-da3f-54fe-3e74-8eefd125c2b0", "type": "Violation", "state": "1", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:43:13.219231+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	\N	postgres	2025-12-18 13:44:43.927257+08
019b2ffd-8f71-7a47-8409-c7ba58ac3e79	complaints	019b2ffd-8295-848c-638e-f2383c72b0ae	I	\N	{"id": "019b2ffd-8295-848c-638e-f2383c72b0ae", "type": "Violation", "state": "已发起", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:45:01.850654+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	postgres	2025-12-18 13:45:05.135405+08
019b2ffe-daba-7619-8685-d1ca676543ee	complaints	019b2ffd-8295-848c-638e-f2383c72b0ae	U	{"id": "019b2ffd-8295-848c-638e-f2383c72b0ae", "type": "Violation", "state": "已发起", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:45:01.850654+08:00", "handle_time": null, "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": null}	{"id": "019b2ffd-8295-848c-638e-f2383c72b0ae", "type": "Violation", "state": "已处理", "content": "管理员", "seat_id": "019b0bf4-09ec-a829-d6ce-2bf561e73301", "create_time": "2025-12-18T13:45:01.850654+08:00", "handle_time": "2025-12-18T13:46:27.263017+08:00", "send_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "handle_user_id": "019b02b2-5a66-1a39-409c-3cc84569706d"}	postgres	2025-12-18 13:46:29.945769+08
019b3004-74f3-7e91-86cd-3a453b8343a4	users	019b0818-6889-9081-34a3-d1da36233462	U	{"id": "019b0818-6889-9081-34a3-d1da36233462", "role": "User", "email": null, "phone": "17843030218", "avatar": null, "password": "$2a$11$c454T7Yqi29ScYOQYESJg.iJKlrJD5C58xXpAOdkNRMj/jzyeFq32", "campus_id": "236003921", "user_name": "testuser001", "create_time": "2025-12-10T19:49:36.008974+08:00", "display_name": "testuser001"}	{"id": "019b0818-6889-9081-34a3-d1da36233462", "role": "User", "email": "2590181135@qq.com", "phone": "17843030218", "avatar": null, "password": "$2a$11$c454T7Yqi29ScYOQYESJg.iJKlrJD5C58xXpAOdkNRMj/jzyeFq32", "campus_id": "236003921", "user_name": "testuser001", "create_time": "2025-12-10T19:49:36.008974+08:00", "display_name": "testuser001"}	postgres	2025-12-18 13:52:37.10643+08
019b3004-dedb-7ef3-a7c4-098e68f851dd	bookings	019b3004-ddd5-34ea-4ce6-365337a3f3a0	I	\N	{"id": "019b3004-ddd5-34ea-4ce6-365337a3f3a0", "state": "Booked", "seat_id": "019b0c31-bfc6-f030-5446-63149a76d12f", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-18T14:30:00+08:00", "start_time": "2025-12-18T14:00:00+08:00", "create_time": "2025-12-18T13:53:03.958044+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 13:53:04.218296+08
019b3004-fa38-7685-be30-d5276b4693b2	bookings	019b3004-ddd5-34ea-4ce6-365337a3f3a0	U	{"id": "019b3004-ddd5-34ea-4ce6-365337a3f3a0", "state": "Booked", "seat_id": "019b0c31-bfc6-f030-5446-63149a76d12f", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-18T14:30:00+08:00", "start_time": "2025-12-18T14:00:00+08:00", "create_time": "2025-12-18T13:53:03.958044+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b3004-ddd5-34ea-4ce6-365337a3f3a0", "state": "CheckIn", "seat_id": "019b0c31-bfc6-f030-5446-63149a76d12f", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-18T14:30:00+08:00", "start_time": "2025-12-18T14:00:00+08:00", "create_time": "2025-12-18T13:53:03.958044+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 13:53:11.223977+08
019b3005-b779-772e-b788-102a79089eb3	users	019b3005-b548-5d4c-ecfa-3fa34aff0d73	I	\N	{"id": "019b3005-b548-5d4c-ecfa-3fa34aff0d73", "role": "User", "email": null, "phone": "17878787878", "avatar": null, "password": "$2a$11$YEBngFbIOzD/rTmKwqEAZu50BvZVebe1pYF.t8X8EfTXFDQ2d8fg6", "campus_id": "256053921", "user_name": "testuser154", "create_time": "2025-12-18T13:53:59.112767+08:00", "display_name": "testuser154"}	postgres	2025-12-18 13:53:59.671912+08
019b3008-cea7-756c-bdb2-4243af061980	users	019b3008-cc6b-aaed-b6ad-6588b2d4c131	I	\N	{"id": "019b3008-cc6b-aaed-b6ad-6588b2d4c131", "role": "Admin", "email": null, "phone": "17876767676", "avatar": null, "password": "$2a$11$/s9Z6U8XG8qib/pCF9mUF.GUcrVuqAAe8FY65z0KM9przzoI8Ovu2", "campus_id": "254003921", "user_name": "testuser156", "create_time": "2025-12-18T13:57:21.643822+08:00", "display_name": "testuser156"}	postgres	2025-12-18 13:57:22.213999+08
019b3016-b0c2-77f0-80f5-7329256f1d6c	bookings	019b3016-afa4-43d3-7a23-14c9010e0c55	I	\N	{"id": "019b3016-afa4-43d3-7a23-14c9010e0c55", "state": "Booked", "seat_id": "019b0bf4-08cc-c971-839d-87b46a8f5040", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T19:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:12:31.791162+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 14:12:32.064309+08
019b3016-c078-73e6-9fe3-09c81c285a64	bookings	019b3016-c015-b51c-1693-dbdee7291662	I	\N	{"id": "019b3016-c015-b51c-1693-dbdee7291662", "state": "Booked", "seat_id": "019b0bf4-0da4-0d4b-6bba-f12c536f3c5d", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T19:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:12:35.989463+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 14:12:36.087718+08
019b3016-cf23-75cb-890f-f5372dab71da	bookings	019b3016-ceae-1a91-945d-8acd4ae462f2	I	\N	{"id": "019b3016-ceae-1a91-945d-8acd4ae462f2", "state": "Booked", "seat_id": "019b0bf4-0fe4-6d53-9f75-3a6b5a1fa20b", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T19:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:12:39.726915+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 14:12:39.842855+08
019b3016-e100-7125-9d24-a946e819cb2b	bookings	019b3016-e090-890c-ad50-83ba04445f60	I	\N	{"id": "019b3016-e090-890c-ad50-83ba04445f60", "state": "Booked", "seat_id": "019b0bf4-0ae0-dd4b-8fe3-d90dc5245642", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T19:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:12:44.304897+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 14:12:44.414285+08
019b301d-d3f3-78f7-bcfe-47bd06caec7d	bookings	019b3004-ddd5-34ea-4ce6-365337a3f3a0	U	{"id": "019b3004-ddd5-34ea-4ce6-365337a3f3a0", "state": "CheckIn", "seat_id": "019b0c31-bfc6-f030-5446-63149a76d12f", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-18T14:30:00+08:00", "start_time": "2025-12-18T14:00:00+08:00", "create_time": "2025-12-18T13:53:03.958044+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b3004-ddd5-34ea-4ce6-365337a3f3a0", "state": "Checkout", "seat_id": "019b0c31-bfc6-f030-5446-63149a76d12f", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-18T14:30:00+08:00", "start_time": "2025-12-18T14:00:00+08:00", "create_time": "2025-12-18T13:53:03.958044+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 14:20:19.825982+08
019b303b-24e3-747c-8c9e-a2ccad3dbdd8	bookings	019b303b-2470-2bf9-9da7-3bc63b7ec86c	I	\N	{"id": "019b303b-2470-2bf9-9da7-3bc63b7ec86c", "state": "Booked", "seat_id": "019b1ca2-edbb-0df7-35a8-633c02733a74", "user_id": "019b0887-0e14-97cb-9739-66ead959acf1", "end_time": "2025-12-18T20:00:00+08:00", "start_time": "2025-12-18T16:00:00+08:00", "create_time": "2025-12-18T14:52:20.976304+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 14:52:21.089+08
019b303c-40db-754b-8b64-0fd21e082149	bookings	019b303c-406a-9bbb-2093-9f46af5e0ccd	I	\N	{"id": "019b303c-406a-9bbb-2093-9f46af5e0ccd", "state": "Booked", "seat_id": "019b0bf4-08c2-da78-d0f3-593cc6bae46a", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T17:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:53:33.67442+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 14:53:33.785402+08
019b3045-c465-7b7a-b60c-47c79bb00fc4	bookings	019b303c-406a-9bbb-2093-9f46af5e0ccd	U	{"id": "019b303c-406a-9bbb-2093-9f46af5e0ccd", "state": "Booked", "seat_id": "019b0bf4-08c2-da78-d0f3-593cc6bae46a", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T17:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:53:33.67442+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b303c-406a-9bbb-2093-9f46af5e0ccd", "state": "CheckIn", "seat_id": "019b0bf4-08c2-da78-d0f3-593cc6bae46a", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T17:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:53:33.67442+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 15:03:57.284288+08
019b3045-d134-71fc-92db-d00d49c8a8d0	bookings	019b3016-e090-890c-ad50-83ba04445f60	U	{"id": "019b3016-e090-890c-ad50-83ba04445f60", "state": "Booked", "seat_id": "019b0bf4-0ae0-dd4b-8fe3-d90dc5245642", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T19:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:12:44.304897+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b3016-e090-890c-ad50-83ba04445f60", "state": "CheckIn", "seat_id": "019b0bf4-0ae0-dd4b-8fe3-d90dc5245642", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T19:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:12:44.304897+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 15:04:00.562654+08
019b3045-db17-765e-af67-9fe47e691514	bookings	019b3016-ceae-1a91-945d-8acd4ae462f2	U	{"id": "019b3016-ceae-1a91-945d-8acd4ae462f2", "state": "Booked", "seat_id": "019b0bf4-0fe4-6d53-9f75-3a6b5a1fa20b", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T19:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:12:39.726915+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b3016-ceae-1a91-945d-8acd4ae462f2", "state": "CheckIn", "seat_id": "019b0bf4-0fe4-6d53-9f75-3a6b5a1fa20b", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T19:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:12:39.726915+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 15:04:03.095047+08
019b3045-e384-77e4-8546-7a6f92527640	bookings	019b3016-c015-b51c-1693-dbdee7291662	U	{"id": "019b3016-c015-b51c-1693-dbdee7291662", "state": "Booked", "seat_id": "019b0bf4-0da4-0d4b-6bba-f12c536f3c5d", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T19:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:12:35.989463+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b3016-c015-b51c-1693-dbdee7291662", "state": "CheckIn", "seat_id": "019b0bf4-0da4-0d4b-6bba-f12c536f3c5d", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T19:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:12:35.989463+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 15:04:05.252142+08
019b304f-e468-78ed-b184-72ddcbdfa113	bookings	019b3016-afa4-43d3-7a23-14c9010e0c55	U	{"id": "019b3016-afa4-43d3-7a23-14c9010e0c55", "state": "Booked", "seat_id": "019b0bf4-08cc-c971-839d-87b46a8f5040", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T19:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:12:31.791162+08:00", "check_in_time": null, "check_out_time": null}	{"id": "019b3016-afa4-43d3-7a23-14c9010e0c55", "state": "Canceled", "seat_id": "019b0bf4-08cc-c971-839d-87b46a8f5040", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "end_time": "2025-12-18T19:00:00+08:00", "start_time": "2025-12-18T15:00:00+08:00", "create_time": "2025-12-18T14:12:31.791162+08:00", "check_in_time": null, "check_out_time": null}	postgres	2025-12-18 15:15:00.83891+08
019b304f-e4ed-7f4b-94f3-14997bdb7404	violations	019b304f-e450-04af-922b-f9afc0b1403c	I	\N	{"id": "019b304f-e450-04af-922b-f9afc0b1403c", "type": "超时", "state": "Violation", "content": "未在规定时间签到", "user_id": "019b02b2-5a66-1a39-409c-3cc84569706d", "booking_id": "019b3016-afa4-43d3-7a23-14c9010e0c55", "create_time": "2025-12-18T15:15:00.817203+08:00"}	postgres	2025-12-18 15:15:00.972758+08
\.


--
-- Data for Name: blacklists; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.blacklists (id, user_id, type, reason, create_time, expire_time) FROM stdin;
\.


--
-- Data for Name: bookings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.bookings (id, user_id, seat_id, create_time, start_time, end_time, check_in_time, check_out_time, state) FROM stdin;
019b088a-d9c9-ec25-85d3-e0e6bb1ea581	019b0887-0e14-97cb-9739-66ead959acf1	019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	2025-12-10 21:54:36.105166+08	2025-12-16 10:50:00+08	2025-12-16 13:45:00+08	\N	\N	Canceled
019b0c2b-3c64-63ba-2677-999eacb03923	019b0887-0e14-97cb-9739-66ead959acf1	019b0bf4-0552-b61d-d511-2d39856f5fc1	2025-12-11 14:48:38.756484+08	2025-12-25 09:30:00+08	2025-12-25 10:50:00+08	\N	\N	Canceled
019b0c66-af95-643c-638f-6e2cd5beead1	019b0818-6889-9081-34a3-d1da36233462	019b0bf4-036e-0e48-a96a-7277084ec5a2	2025-12-11 15:53:34.872183+08	2025-12-19 10:00:00+08	2025-12-19 12:00:00+08	\N	\N	Booked
019b2f2a-703b-5d87-c96f-be081a2a7b4d	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-0555-117d-eb34-e0d634c38793	2025-12-18 09:54:29.062756+08	2025-12-18 09:00:00+08	2025-12-18 09:30:00+08	\N	\N	Canceled
019b2f2c-9214-0952-a297-f649f9b0b87f	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-036e-0e48-a96a-7277084ec5a2	2025-12-18 09:56:48.788464+08	2025-12-18 10:00:00+08	2025-12-18 11:00:00+08	\N	\N	Canceled
019b0d15-96e7-df72-5109-9b5645b2e93b	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-0c71-b7ad-8c07-18cbf5a44d79	2025-12-11 19:04:37.35718+08	2025-12-11 19:10:00+08	2025-12-11 19:20:00+08	\N	\N	Checkout
019b3004-ddd5-34ea-4ce6-365337a3f3a0	019b0887-0e14-97cb-9739-66ead959acf1	019b0c31-bfc6-f030-5446-63149a76d12f	2025-12-18 13:53:03.958044+08	2025-12-18 14:00:00+08	2025-12-18 14:30:00+08	\N	\N	Checkout
019b303b-2470-2bf9-9da7-3bc63b7ec86c	019b0887-0e14-97cb-9739-66ead959acf1	019b1ca2-edbb-0df7-35a8-633c02733a74	2025-12-18 14:52:20.976304+08	2025-12-18 16:00:00+08	2025-12-18 20:00:00+08	\N	\N	Booked
019b1cdf-bc88-5ed0-a1d6-837c65097529	019b0887-0e14-97cb-9739-66ead959acf1	019b0bf4-036e-0e48-a96a-7277084ec5a2	2025-12-14 20:39:43.496399+08	2025-12-14 21:00:00+08	2025-12-14 22:00:00+08	\N	\N	Checkout
019b1813-3860-b0e1-2c5c-0e64606a3124	019b0887-0e14-97cb-9739-66ead959acf1	019b0c91-3d13-cd9b-730b-b600bddfce4e	2025-12-13 22:17:51.457721+08	2025-12-25 11:30:00+08	2025-12-25 13:30:00+08	\N	\N	Canceled
019b1c83-f379-7724-f83f-0edb381ff83e	019b0887-0e14-97cb-9739-66ead959acf1	019b0bf4-036e-0e48-a96a-7277084ec5a2	2025-12-14 18:59:28.250344+08	2025-12-31 13:00:00+08	2025-12-31 14:00:00+08	\N	\N	Canceled
019b1c85-f5f9-19cb-baea-8edeb667be29	019b0887-0e14-97cb-9739-66ead959acf1	019b0c31-c0f0-5f3c-56fd-ccff5fe25310	2025-12-14 19:01:39.961449+08	2025-12-31 13:00:00+08	2025-12-31 14:00:00+08	\N	\N	Canceled
019b1cd3-7d8d-7a03-2524-a8e269708f29	019b0887-0e14-97cb-9739-66ead959acf1	019b0bf4-036e-0e48-a96a-7277084ec5a2	2025-12-14 20:26:20.941513+08	2025-12-15 07:30:00+08	2025-12-15 08:30:00+08	\N	\N	Canceled
019b303c-406a-9bbb-2093-9f46af5e0ccd	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-08c2-da78-d0f3-593cc6bae46a	2025-12-18 14:53:33.67442+08	2025-12-18 15:00:00+08	2025-12-18 17:00:00+08	\N	\N	CheckIn
019b3016-e090-890c-ad50-83ba04445f60	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-0ae0-dd4b-8fe3-d90dc5245642	2025-12-18 14:12:44.304897+08	2025-12-18 15:00:00+08	2025-12-18 19:00:00+08	\N	\N	CheckIn
019b3016-ceae-1a91-945d-8acd4ae462f2	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-0fe4-6d53-9f75-3a6b5a1fa20b	2025-12-18 14:12:39.726915+08	2025-12-18 15:00:00+08	2025-12-18 19:00:00+08	\N	\N	CheckIn
019b3016-c015-b51c-1693-dbdee7291662	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-0da4-0d4b-6bba-f12c536f3c5d	2025-12-18 14:12:35.989463+08	2025-12-18 15:00:00+08	2025-12-18 19:00:00+08	\N	\N	CheckIn
019b3016-afa4-43d3-7a23-14c9010e0c55	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-08cc-c971-839d-87b46a8f5040	2025-12-18 14:12:31.791162+08	2025-12-18 15:00:00+08	2025-12-18 19:00:00+08	\N	\N	Canceled
019afbd2-28db-c438-69bd-021b3fd06b63	019a1b60-de17-ead2-3902-26fb51efc5bf	019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	2025-12-08 10:37:25.60505+08	2025-12-08 22:14:14.873824+08	2025-12-08 23:14:14.873824+08	\N	\N	Canceled
019afbd6-8723-d5e2-11ff-9ff5fab254c2	019a1b60-de17-ead2-3902-26fb51efc5bf	019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	2025-12-08 10:42:11.882352+08	2025-12-08 20:22:54.336911+08	2025-12-08 21:22:54.336911+08	\N	\N	Canceled
019b07ea-968c-4138-4d88-18066a779be8	019b02b2-5a66-1a39-409c-3cc84569706d	019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	2025-12-10 18:59:33.13445+08	2025-12-11 16:58:22.303866+08	2025-12-11 18:58:22.303866+08	\N	\N	Canceled
019b0b75-fe6e-9ced-0d10-ae2a5f24a1a6	019b0887-0e14-97cb-9739-66ead959acf1	019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	2025-12-11 11:30:40.878246+08	2025-12-10 10:40:00+08	2025-12-10 12:55:00+08	\N	\N	Canceled
019b0c15-8355-5686-007e-8be225bf6d9a	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-0caa-f408-cd73-902810499248	2025-12-11 14:24:55.12519+08	2025-12-11 12:40:00+08	2025-12-11 14:40:00+08	\N	\N	Canceled
019b0c27-fda8-67cc-187b-27ef29c6b804	019b0887-0e14-97cb-9739-66ead959acf1	019b0bf4-03f0-03e6-7063-73e2354db38a	2025-12-11 14:45:06.08866+08	2025-12-01 08:40:00+08	2025-12-01 11:45:00+08	\N	\N	Canceled
019b0c67-0984-9ca7-a8d3-1236c9f61e19	019b0887-0e14-97cb-9739-66ead959acf1	019b0bf4-036e-0e48-a96a-7277084ec5a2	2025-12-11 15:53:57.892685+08	2025-12-01 10:40:00+08	2025-12-01 13:50:00+08	\N	\N	Canceled
019b0c6a-5e86-26d4-4544-29e5e89f2acf	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-036e-0e48-a96a-7277084ec5a2	2025-12-11 15:57:36.262531+08	2025-12-12 10:00:00+08	2025-12-12 11:00:00+08	\N	\N	Canceled
019b0c6c-0288-6927-2048-734a88ae12e1	019b0887-0e14-97cb-9739-66ead959acf1	019b0c31-bfc6-f030-5446-63149a76d12f	2025-12-11 15:59:23.78425+08	2025-12-11 12:00:00+08	2025-12-11 13:00:00+08	\N	\N	Canceled
019b0c88-8f5a-e86d-a4ea-851458b2db98	019b0818-6889-9081-34a3-d1da36233462	019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	2025-12-11 16:30:34.842854+08	2025-12-12 10:00:00+08	2025-12-12 10:05:00+08	\N	\N	Canceled
019b0c8f-20f1-2e0c-2321-b56a3ba682ca	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-036e-0e48-a96a-7277084ec5a2	2025-12-11 16:37:45.329849+08	2025-12-12 13:00:00+08	2025-12-12 15:00:00+08	\N	\N	Canceled
019b0d5b-e385-5bb3-5414-9834ae6557a1	019b0818-6889-9081-34a3-d1da36233462	019b0bf4-036e-0e48-a96a-7277084ec5a2	2025-12-11 20:21:24.489061+08	2025-12-13 10:00:00+08	2025-12-13 12:00:00+08	\N	\N	Canceled
019b2180-93c2-831b-6e94-54538bfbf665	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-036e-0e48-a96a-7277084ec5a2	2025-12-15 18:13:53.229478+08	2025-12-16 11:00:00+08	2025-12-16 12:00:00+08	\N	\N	Canceled
019b1d26-c56f-c648-536b-cd5c1e79d7fc	019b0887-0e14-97cb-9739-66ead959acf1	019b0bf4-03f0-03e6-7063-73e2354db38a	2025-12-14 21:57:18.831877+08	2025-12-31 08:00:00+08	2025-12-31 20:00:00+08	\N	\N	Booked
\.


--
-- Data for Name: complaints; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.complaints (id, send_user_id, state, type, send_content, create_time, handle_time, handle_user_id, seat_id, handle_content, target_time) FROM stdin;
019b2ffd-8295-848c-638e-f2383c72b0ae	019b02b2-5a66-1a39-409c-3cc84569706d	已处理	Violation	管理员	2025-12-18 13:45:01.850654+08	2025-12-18 13:46:27.263017+08	019b02b2-5a66-1a39-409c-3cc84569706d	019b0bf4-09ec-a829-d6ce-2bf561e73301	\N	\N
\.


--
-- Data for Name: rooms; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.rooms (id, name, open_time, close_time, rows, cols) FROM stdin;
019a1e1a-0802-6976-c80c-60c92a59c0ad	101	07:00:00	22:00:00	5	8
019b0c91-9c5f-0704-04d2-8c0aece24d22	103	08:00:00	22:00:00	6	8
019b17e5-ffe1-aba8-5573-c71eae08c95d	104	08:00:00	22:00:00	4	4
019b17e6-1aa0-5f9c-0680-156d843b1c62	105	08:00:00	22:00:00	4	4
019b17e6-2eb0-779a-a733-53118ace14a4	106	08:00:00	22:00:00	4	4
019b17e7-1ee2-75b0-30ef-847c3bdb26de	107	08:00:00	22:00:00	4	4
019a204d-2693-b609-dfd8-90af43016dba	102	06:30:00	21:00:00	4	7
\.


--
-- Data for Name: seats; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.seats (id, room_id, "row", col) FROM stdin;
019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	019a1e1a-0802-6976-c80c-60c92a59c0ad	1	2
019a1e1b-c9be-aa61-f0c9-7909ca47fa43	019a1e1a-0802-6976-c80c-60c92a59c0ad	3	3
019a2531-961d-f2a0-9e98-cd0038609c9c	019a1e1a-0802-6976-c80c-60c92a59c0ad	3	2
019b0bf4-03f0-03e6-7063-73e2354db38a	019a1e1a-0802-6976-c80c-60c92a59c0ad	0	1
019b0bf4-036e-0e48-a96a-7277084ec5a2	019a1e1a-0802-6976-c80c-60c92a59c0ad	0	0
019b0bf4-050e-2362-9a4d-596dc760bbbc	019a1e1a-0802-6976-c80c-60c92a59c0ad	0	5
019b0bf4-054e-1d3a-872d-c5b7d663646d	019a1e1a-0802-6976-c80c-60c92a59c0ad	0	4
019b0bf4-0552-b61d-d511-2d39856f5fc1	019a1e1a-0802-6976-c80c-60c92a59c0ad	0	2
019b0bf4-0555-117d-eb34-e0d634c38793	019a1e1a-0802-6976-c80c-60c92a59c0ad	0	3
019b0bf4-069a-5a7f-3382-db90b3a2965c	019a1e1a-0802-6976-c80c-60c92a59c0ad	0	6
019b0bf4-07d9-fe16-4a1e-f6e18bbbb8f1	019a1e1a-0802-6976-c80c-60c92a59c0ad	1	0
019b0bf4-07f5-4b3c-47c2-79665eee4077	019a1e1a-0802-6976-c80c-60c92a59c0ad	1	1
019b0bf4-08c2-da78-d0f3-593cc6bae46a	019a1e1a-0802-6976-c80c-60c92a59c0ad	1	4
019b0bf4-08cc-c971-839d-87b46a8f5040	019a1e1a-0802-6976-c80c-60c92a59c0ad	1	3
019b0bf4-09ec-a829-d6ce-2bf561e73301	019a1e1a-0802-6976-c80c-60c92a59c0ad	1	5
019b0bf4-0ae0-dd4b-8fe3-d90dc5245642	019a1e1a-0802-6976-c80c-60c92a59c0ad	1	6
019b0bf4-0bd4-0d97-05e0-a4773e0152d7	019a1e1a-0802-6976-c80c-60c92a59c0ad	2	0
019b0bf4-0c69-1db0-c2fe-d6ac6a556ad1	019a1e1a-0802-6976-c80c-60c92a59c0ad	2	1
019b0bf4-0c71-b7ad-8c07-18cbf5a44d79	019a1e1a-0802-6976-c80c-60c92a59c0ad	2	2
019b0bf4-0caa-f408-cd73-902810499248	019a1e1a-0802-6976-c80c-60c92a59c0ad	2	3
019b0bf4-0da4-0d4b-6bba-f12c536f3c5d	019a1e1a-0802-6976-c80c-60c92a59c0ad	2	4
019b0bf4-0db6-d747-8107-99f620f45fd4	019a1e1a-0802-6976-c80c-60c92a59c0ad	2	5
019b0bf4-0ec0-62ca-1fea-1845d383c817	019a1e1a-0802-6976-c80c-60c92a59c0ad	2	6
019b0bf4-0f5f-84c3-048f-14e011a4f8de	019a1e1a-0802-6976-c80c-60c92a59c0ad	2	7
019b0bf4-0f9f-ac99-8baa-706bd5f57076	019a1e1a-0802-6976-c80c-60c92a59c0ad	3	0
019b0bf4-0fe4-6d53-9f75-3a6b5a1fa20b	019a1e1a-0802-6976-c80c-60c92a59c0ad	3	1
019b0bf4-1107-87c1-863e-aea48175d6da	019a1e1a-0802-6976-c80c-60c92a59c0ad	3	5
019b0bf4-1147-42ba-0f88-38d789e6efba	019a1e1a-0802-6976-c80c-60c92a59c0ad	3	4
019b0bf4-1253-38a5-2434-25ac470da06b	019a1e1a-0802-6976-c80c-60c92a59c0ad	3	6
019b0bf4-1297-8224-b0bf-d0f4eba168df	019a1e1a-0802-6976-c80c-60c92a59c0ad	3	7
019b0bf4-13a0-0320-1349-0b4c1b219fed	019a1e1a-0802-6976-c80c-60c92a59c0ad	4	1
019b0bf4-13a4-b779-668c-bf9872bc6646	019a1e1a-0802-6976-c80c-60c92a59c0ad	4	0
019b0bf4-14ae-b903-d652-31fd03a074f4	019a1e1a-0802-6976-c80c-60c92a59c0ad	4	2
019b0bf4-14d2-7317-a4ee-c9e26fbb5d2d	019a1e1a-0802-6976-c80c-60c92a59c0ad	4	3
019b0bf4-15ce-c831-22b5-ea9194035470	019a1e1a-0802-6976-c80c-60c92a59c0ad	4	4
019b0bf4-162f-faa3-d588-97093c131663	019a1e1a-0802-6976-c80c-60c92a59c0ad	4	5
019b0bf4-16bc-307c-02ba-5975c27b3a66	019a1e1a-0802-6976-c80c-60c92a59c0ad	4	6
019b0bf4-29b2-6c4c-51f5-77ff2f12eb27	019a1e1a-0802-6976-c80c-60c92a59c0ad	4	7
019b0bf4-c949-fc7d-bd3a-102ac87d8c58	019a1e1a-0802-6976-c80c-60c92a59c0ad	0	7
019b0bf4-c952-adf8-7a89-75dba93a8779	019a1e1a-0802-6976-c80c-60c92a59c0ad	1	7
019b0c31-bfc6-f030-5446-63149a76d12f	019a204d-2693-b609-dfd8-90af43016dba	0	2
019b0c31-c0f0-5f3c-56fd-ccff5fe25310	019a204d-2693-b609-dfd8-90af43016dba	0	3
019b0c31-c1b3-628c-3cf7-16391f2fef43	019a204d-2693-b609-dfd8-90af43016dba	2	2
019b0c31-c1ca-7d8f-654d-f2c79d27c3e6	019a204d-2693-b609-dfd8-90af43016dba	1	2
019b0c84-3012-e14a-fcd7-f9c40c5fae54	019a204d-2693-b609-dfd8-90af43016dba	1	3
019b0c84-3010-a6e3-be3d-e2950871e11d	019a204d-2693-b609-dfd8-90af43016dba	2	3
019b0c84-315c-cf9c-32bb-91cf18c4be7a	019a204d-2693-b609-dfd8-90af43016dba	3	2
019b0c84-3157-e820-c27b-8aeae91eedc3	019a204d-2693-b609-dfd8-90af43016dba	3	3
019b0c91-3d13-cd9b-730b-b600bddfce4e	019a204d-2693-b609-dfd8-90af43016dba	0	4
019b0c91-3e10-ca66-cb96-23850cdc2825	019a204d-2693-b609-dfd8-90af43016dba	1	4
019b1ca2-eb2b-b2d6-7552-b53039bacd6a	019b17e5-ffe1-aba8-5573-c71eae08c95d	0	0
019b1ca2-ec83-8da2-7f83-4f435d2d71a5	019b17e5-ffe1-aba8-5573-c71eae08c95d	0	3
019b1ca2-edac-e17b-a4eb-f33c07c625e9	019b17e5-ffe1-aba8-5573-c71eae08c95d	2	2
019b1ca2-edbb-0df7-35a8-633c02733a74	019b17e5-ffe1-aba8-5573-c71eae08c95d	1	2
019b1ca2-edc9-f176-f67d-9c32aa8cf7cb	019b17e5-ffe1-aba8-5573-c71eae08c95d	1	1
019b1ca2-edce-c166-420b-ad9904774207	019b17e5-ffe1-aba8-5573-c71eae08c95d	2	1
019b1ca2-edf9-173e-8d7d-260fcfb717b3	019b17e5-ffe1-aba8-5573-c71eae08c95d	3	0
019b1ca2-eead-525f-294d-fa6ebe64ad43	019b17e5-ffe1-aba8-5573-c71eae08c95d	3	3
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, create_time, user_name, display_name, password, campus_id, phone, email, role, avatar) FROM stdin;
019a1b60-de17-ead2-3902-26fb51efc5bf	2025-10-25 20:38:44.50294+08	zengkun	ZengKun	$2a$11$whcOhV3mebB7dS/b1zad3eyEasRM0gRR3KtbnELfNU5X9DLpAWJAi	236001209	15336567166	1489782679@qq.com	Admin	/api/v1/file/019a7aa9-27e7-2607-6c94-d2a3946fd8d6.png
019b02aa-3286-29a3-b200-86be59bb04c7	2025-12-09 18:31:07.141937+08	Mr.Zhao	Mr.Zhao	$2a$11$xt0aIjl0X0CMRq96et8Ew.zohRknOzCk./vYwNtg8Oei8NabVdGOa	236001111	16611111111	\N	User	\N
019b0818-6889-9081-34a3-d1da36233462	2025-12-10 19:49:36.008974+08	testuser001	testuser001	$2a$11$c454T7Yqi29ScYOQYESJg.iJKlrJD5C58xXpAOdkNRMj/jzyeFq32	236003921	17843030218	2590181135@qq.com	User	\N
019b0814-12d4-17c7-3189-c81dc3e2247e	2025-12-10 19:44:51.924105+08	aord	Aord	$2a$11$drugRKxrprmkzLBR/ryln.W6DPXTScm4p4ZCtI0XmSWWvP07Fea8y	234001112	14511111112	123457@123.com	User	\N
019b0838-8501-4f16-57ea-e8b816d3f87d	2025-12-10 20:24:40.449605+08	tsetuser021	tsetuser021	$2a$11$Rc///j.8hxMyL7aC4hMvgOcs1NS.XNqGZnlgsg8GY85tPwMKgfgya	226003921	17843030211	\N	User	\N
019b0848-9852-e8d8-becd-d852a322e439	2025-12-10 20:42:13.970791+08	testuser0022	testuser0022	$2a$11$bw1d3oncq8ysAkNYQSM8wuUA4N7oZBSiMrXCayk/hXF/WSiBIBc2q	236003928	17843020218	\N	User	\N
019b084a-35f1-502c-d286-578e2894e468	2025-12-10 20:43:59.857946+08	testuser0023	testuser0023	$2a$11$QF/h/Wv7d3jlIxKuyKeoGO1BI9wlXl9ZfxvOdrfE5/rgHPaR9gC0u	225003921	17845030218	\N	User	\N
019b085f-11be-2c97-2c8e-83ce45637d00	2025-12-10 21:06:46.846187+08	Mr.Zhao1	Mr.Zhao1	$2a$11$/o7dORKMxJCRvyB5Vt3uf.GrakcFMjxb6qjOt4mC/jsd5Ki6RRgEi	236001100	16611111100	\N	User	\N
019b0877-179d-2192-6074-44c4fe225bb1	2025-12-10 21:33:01.21368+08	' OR '1'='1	' OR '1'='1	$2a$11$i3IfdXb6fq6EggcNrDySmulRs7sQ3MkAqqS4xtQ8PEdnBzeCHMK06	74wyav1uv0	16324069067	\N	User	\N
019b0877-1b60-427b-2f82-a7fd960de77e	2025-12-10 21:33:02.176281+08	" OR "1"="1	" OR "1"="1	$2a$11$NDLKNm31j9QhCxGpngNgwOkmEcznjQ89y95F8PIxFlQ2EYQ39WhGm	6ovv736g4a	18358448518	\N	User	\N
019b0877-1eec-d2ae-3170-6b17fc7585dd	2025-12-10 21:33:03.084289+08	'; DROP TABLE users; --	'; DROP TABLE users; --	$2a$11$SoOurPRVCTL7yFdJzq1hDeWwPhn71XtGy7J4DePvgxuFImaTbToh6	y0lb97rhcm	17520864592	\N	User	\N
019b0877-2273-ba4f-f37f-a8c6ea92d483	2025-12-10 21:33:03.987427+08	admin'--	admin'--	$2a$11$VU6fhGJXKwyyF5z97x168evWfRq40as9cQ1bc12fYEmDnyhKjUcu2	dsycwl83tn	17780684508	\N	User	\N
019b0877-25f4-b0cd-ba89-84519b0a63bd	2025-12-10 21:33:04.884063+08	admin'/*	admin'/*	$2a$11$OeL0CH3YAZrYgJvkL5.hHOL/skfgma0rG3XHBOb.IknWS.eeA9e.C	ruzvkknohc	17721494483	\N	User	\N
019b0877-2972-0501-c482-6e5718907987	2025-12-10 21:33:05.778861+08	' UNION SELECT * FROM users--	' UNION SELECT * FROM users--	$2a$11$suenrrNHX4qsFy5W7SO2Y.j7zPtWUB0eljLqgyakXn6JMVcrfwrlm	s2ztc9l8tw	19809203568	\N	User	\N
019b0877-2d06-90b4-6e95-54040b41c47c	2025-12-10 21:33:06.694634+08	' OR 1=1--	' OR 1=1--	$2a$11$/U3hqASQU3pm3X/Kndon7O/2QbpbVz.aFN9h2SL.Hme84pTD0jC0S	qctfnfxe5p	14922872118	\N	User	\N
019b0877-308a-e5a6-0f85-93b19f1d83be	2025-12-10 21:33:07.594558+08	' OR 1=2--	' OR 1=2--	$2a$11$yaNQGFLl5h1p/p9aBesMQOwqtx9TFYEUJ675GwGC/fPKx5blesTzW	ue6gz2xuqv	14913495505	\N	User	\N
019b0877-340a-34f7-027f-af161cfdf8ed	2025-12-10 21:33:08.490977+08	' AND '1'='1	' AND '1'='1	$2a$11$EfdgQjMT9nkh62ZotHIh9eHex9/3GyyiYscdNYKj5TBp36KUirjqm	6q7v3p2jpc	17730602515	\N	User	\N
019b0877-3774-7d76-590d-423718ccbdd6	2025-12-10 21:33:09.364599+08	' AND '1'='2	' AND '1'='2	$2a$11$1.4Y7oggMNbjpnOE9jjH0.B7LoGgJLeYjD/OWeYfC6eM4ZNnTi.T.	3ot5hxccgu	17426969927	\N	User	\N
019b0877-3aec-0e2a-1891-ae6c822f7b46	2025-12-10 21:33:10.252066+08	'; SELECT pg_sleep(5); --	'; SELECT pg_sleep(5); --	$2a$11$p9ZOa.oiq3CGzrlNtouo/e2SlmnXbXhM9FLPA3wRyjqMCT1Q91B42	8owiamjnon	13659863382	\N	User	\N
019b0877-3e6a-16ee-64e3-c225cfd71493	2025-12-10 21:33:11.146342+08	' OR pg_sleep(5)--	' OR pg_sleep(5)--	$2a$11$LwIbqlRblYEhrEEX8p5jbeAZu80W1jp3nleR5s7dZ5mB.X3EBhbbG	23jf7alts3	19882034269	\N	User	\N
019b0877-41c9-8983-2a60-6644a6e8aa87	2025-12-10 21:33:12.009056+08	'; WAITFOR DELAY '00:00:05'; --	'; WAITFOR DELAY '00:00:05'; --	$2a$11$Clhh/C3irPUwrGxcNN349ukcRIpO7kbc.gbSi.Ex8xymEfSJOM8NK	cvqgybzhct	17904966289	\N	User	\N
019b0877-456d-10de-7b17-0302df360929	2025-12-10 21:33:12.94191+08	' UNION SELECT version()--	' UNION SELECT version()--	$2a$11$C7UWrnjr3VY8i7MsB0Pv8u2I9X9Vn0U3b.mwURmw.dmlSpwrRT1K.	59k46brl8t	17693442582	\N	User	\N
019b0877-4916-cc08-7b04-b45a0637e4a0	2025-12-10 21:33:13.878406+08	' UNION SELECT current_database()--	' UNION SELECT current_database()--	$2a$11$6etSnRmFvakzDlcArvEyLOePJF7xEOgUxIjwJlZR0CJqdvvk/rKz6	0nsou0a4nm	15113074033	\N	User	\N
019b0877-4c9e-ae6e-b226-4a7a706fab4f	2025-12-10 21:33:14.782704+08	' UNION SELECT current_user--	' UNION SELECT current_user--	$2a$11$ESDU8sz0.PgYr5K9j46ozOsHHO6Q0kr5TGDnrnM4SAbfzTYoh8Bc6	f8xgzqcbco	16205481619	\N	User	\N
019b0877-5038-0dd3-271a-9e2fff14e4b2	2025-12-10 21:33:15.704869+08	'; INSERT INTO users (username) VALUES ('hacker'); --	'; INSERT INTO users (username) VALUES ('hacker'); --	$2a$11$aF3rS.0f40yVuLuVoh/5xupeWc6BcD8bQppIe7aUUN.SXv.K4DY9y	hesegsv242	16471923067	\N	User	\N
019b0877-53d6-2340-0bf0-d15eeef2c387	2025-12-10 21:33:16.630904+08	'; UPDATE users SET password='hacked'; --	'; UPDATE users SET password='hacked'; --	$2a$11$9A0qmV9qAxYzecXwAEGtE.95kxSf0KC8bRWHG.u0w0dCNQADilVTq	i8cmisknyl	18186863276	\N	User	\N
019b0877-5778-71e1-ddb8-853b84bbf462	2025-12-10 21:33:17.560649+08	%27%20OR%20%271%27%3D%271	%27%20OR%20%271%27%3D%271	$2a$11$a12xZxob68ZmmJ5UtPWX9OWDwlbUXc/C8vvI7IFLqIaAK2gw18616	hm3ikrhwik	17692239796	\N	User	\N
019b0877-5afd-b67a-a6ad-eb3860a6164a	2025-12-10 21:33:18.461281+08	%2527%20OR%20%271%27%3D%271	%2527%20OR%20%271%27%3D%271	$2a$11$YZIHk8XeuRaAR5kENXoPjesKFdWmwxsWdBGvtnfJNo2fOL3IUBUti	6kl4ptf4ul	17410732781	\N	User	\N
019b0877-5e68-19e0-6f99-9479b99086f3	2025-12-10 21:33:19.336711+08	' OR 1=1	' OR 1=1	$2a$11$G/k1zwO.QDLWH2ZBcsb9hOlRxgkYoQkgBR5tkL.fTEvDVC/xOAjLO	284a5iwhee	16682687787	\N	User	\N
019b0877-6217-f8c2-a74e-786c36dbae5b	2025-12-10 21:33:20.279036+08	admin'/**/OR/**/1=1--	admin'/**/OR/**/1=1--	$2a$11$ht4PEIDcy59zXcXyW7NrZu6QTtDA3hQF.Hy8f67Xpd4yQOnqMtTQS	2fc3dw30tp	15616465171	\N	User	\N
019b0877-66c1-a932-1a91-dbaf70fb72ce	2025-12-10 21:33:21.473613+08	admin'||'1'='1	admin'||'1'='1	$2a$11$ApHczp52Ev3i/J9D4pOxiOpVwtPhH1lim.qX7U3jWS14OQRpxAVyG	ht8pgg1viz	13537514977	\N	User	\N
019b0877-6b5e-e762-ef12-c033e2cabf99	2025-12-10 21:33:22.654706+08	admin'+'1'='1	admin'+'1'='1	$2a$11$TUAAvnIds/50WYNEO42mQ.pjpKARd10.xudE9fN.0nuoU5/TCPc6C	suzgkgjnat	18952239624	\N	User	\N
019b0877-6eeb-dd81-6a5f-ffe6d39c8f7a	2025-12-10 21:33:23.56345+08	'; --	'; --	$2a$11$10bAHKz/6/d.0YnnedO4YO/JHLgihrYpjZ39wcyg8R.g7Z8qsuTJa	iopsdlsesu	18427552605	\N	User	\N
019b0877-7288-1f42-39d3-7ee802c2bc92	2025-12-10 21:33:24.488586+08	'; SELECT pg_read_file('/etc/passwd'); --	'; SELECT pg_read_file('/etc/passwd'); --	$2a$11$eINJUDKrqqmMR8i28Gd0xuNjp46Rnrocu0IqESSqh9NTWeIREA/A2	x7jw50df8b	17683068070	\N	User	\N
019b0877-7614-9415-fb83-bcffad0c304d	2025-12-10 21:33:25.396285+08	'; SELECT * FROM information_schema.tables; --	'; SELECT * FROM information_schema.tables; --	$2a$11$9/Etj0NyXtAhvZ4O1/oEQuqr6EgY0Ce5M930RSCG2nmzJtC.R8aKa	mlkvtpk9yl	17177652103	\N	User	\N
019b0877-79c2-c480-35f4-9203c4e0017b	2025-12-10 21:33:26.338485+08	oeadk86h	oeadk86h	$2a$11$4zPZs4uPtiYRZAcNr6L..ORvj1ykZUdKFxOpRgL9aBqdYGxGA7KE6	mv90w1jh6u	16474383148	\N	User	\N
019b0877-7d48-c29a-bf9b-9786eca0af3d	2025-12-10 21:33:27.240965+08	bg3h1ktg	bg3h1ktg	$2a$11$FSGmFnpTNhYACns8tcEb1./ZXREEkthjgHJuehZqNDiii565Mw7cO	ci6y824w6e	16957398725	\N	User	\N
019b0877-80e9-1de9-ae03-2a402cf409ef	2025-12-10 21:33:28.169734+08	hzwui80r	hzwui80r	$2a$11$6kAo8jPfLNbNmi/YahStPencf6GSwRUs182fyFhk/7evu4ujknoF6	55fv9qw70v	13712181420	\N	User	\N
019b0877-84b8-4441-b533-c879a6808f88	2025-12-10 21:33:29.144351+08	z5ggz2st	z5ggz2st	$2a$11$MOEf8aNxIzuloGo9bQuakOHNf5h2DaefRX25dwqvm2QsGyDDyydre	hntkxv8ydb	17624719351	\N	User	\N
019b0877-8831-7f2d-369d-08813f5b056b	2025-12-10 21:33:30.033192+08	12fovgih	12fovgih	$2a$11$F7t2RQDun8LfdLk15Ziys.2a8tp2Qmg1fV1D8QcChCDCxjtiPYYpG	g0rxdgnkrg	14352601099	\N	User	\N
019b0877-8bcf-35a5-aa74-9a080fb4d707	2025-12-10 21:33:30.959364+08	mz26et6v	mz26et6v	$2a$11$n3unQlGtvWVDi6QFdAt90.bXolwS3zlbzXycx0teFNhtmE1.WEtTO	lcmndfjik4	15988887219	\N	User	\N
019b0877-8f59-c101-b16e-90f2fdc43f5b	2025-12-10 21:33:31.865727+08	dlvvegha	dlvvegha	$2a$11$/W8P6AgAyI7ljoolU/OreepOtI0RrnFk4ov031xNDDUXQjekdxCue	6trjortdcy	14723996706	\N	User	\N
019b0877-92f4-1ec4-7bc4-b74292d02591	2025-12-10 21:33:32.788104+08	86kyzcxt	86kyzcxt	$2a$11$h0LpsJ5L2A0iZaVsdbyc3O0gCXOE0SErvnWc1WlP2s3t7fqET2jFi	mr2wjuz9ai	14362959698	\N	User	\N
019b0877-96a8-0753-efe8-a6e37f3981e5	2025-12-10 21:33:33.736574+08	q6x7xiht	q6x7xiht	$2a$11$N16/vOXWQmxPT9qsXb6CWe.eu0nv/fUSkmiiCtOI66Xj/nIMmqS6.	5n6ixlfp3o	19694908038	\N	User	\N
019b0877-9a5b-baf1-ac91-3a889cccc9d1	2025-12-10 21:33:34.683214+08	37ajyfv7	37ajyfv7	$2a$11$VXbPwPgUKtm6h8csGTpgkeyaAh3XjICNgZmIM8eCindN4WuvVHt96	n8g1o6tuck	19672920596	\N	User	\N
019b0877-9dec-20f6-d3be-f8e9616bdaa2	2025-12-10 21:33:35.596058+08	kemti3wk	kemti3wk	$2a$11$l0cKcOf8anbO8zpYUHVWHOCVVwGqPyyOE.eELt0r0MSGGLpSpJgg2	' OR '1'='1	17289976431	\N	User	\N
019b0877-a1ab-8ffe-d31a-f5972946e8af	2025-12-10 21:33:36.555538+08	5zxayhp1	5zxayhp1	$2a$11$ewcGUTSYrqHy1DGerjS/8e8LL/LGCxX9D1IM49ykV2bRGfyTfuodW	" OR "1"="1	16855742036	\N	User	\N
019b0877-a521-6acf-d7ee-b4594c4ce9a8	2025-12-10 21:33:37.441191+08	k66fd22v	k66fd22v	$2a$11$YDngsISm2OptSZpwUtOFPuCxlMAxER.Sz460Hl2J9v9Vm0kCwu0pC	'; DROP TABLE users; --	16191710595	\N	User	\N
019b0877-a8a1-9564-0276-89f4dfe4e1cc	2025-12-10 21:33:38.337394+08	96ir5big	96ir5big	$2a$11$PzTUidF1ueL8GWzHute6Iu3dr2HLnLueggNDu/Jvwge5g4//KlGNK	admin'--	17421510410	\N	User	\N
019b0877-ac36-396e-009c-762f88b4c19a	2025-12-10 21:33:39.254179+08	zbggx7uu	zbggx7uu	$2a$11$S7yXy9KAuhOyTGhgEQBrEOdhXQDsfFtoCuC2VHehvXssevdbUw3Ca	admin'/*	13312780140	\N	User	\N
019b0877-afae-b804-148f-0b421fdf9b0b	2025-12-10 21:33:40.14284+08	r7ggvelf	r7ggvelf	$2a$11$.7t.bcmoQk15HuXGKIt3w.NMJGAnAgJCkjvQQLxICKbtwmiKVmTGe	' UNION SELECT * FROM users--	15961589507	\N	User	\N
019b0877-b34a-aa65-f254-2ac7d912189b	2025-12-10 21:33:41.066121+08	qip1tsmf	qip1tsmf	$2a$11$vTpPAVJLNkNxse0SnjItg..mfGOxfxvgh5u3XTUVNoRBIdzr01F6G	' OR 1=1--	16958839156	\N	User	\N
019b0877-b6ed-16e3-22e7-29a28af0c4cd	2025-12-10 21:33:41.997121+08	tt8eokia	tt8eokia	$2a$11$FlbB4Z5NUodEZKNpTF58a.42fwO4jNxWprfmzbV9.CZbVwirwl6Xi	' OR 1=2--	15234861706	\N	User	\N
019b0877-ba8f-191b-d251-e14d79a463fa	2025-12-10 21:33:42.927621+08	gcze95ic	gcze95ic	$2a$11$XCbVUllt.mnsejt0XmKgseVAirTyVKpTmEJCMmd845SIV/d2s6Oqy	' AND '1'='1	13507532879	\N	User	\N
019b0877-be3d-0e1c-8397-5564deb3e421	2025-12-10 21:33:43.869895+08	qr0y4ldf	qr0y4ldf	$2a$11$kV8TGMPBNbWkejWg8erf2OTOynWInPwuMpPTtLaP0PeAc11BOePia	' AND '1'='2	19865663951	\N	User	\N
019b0877-d6f9-2819-ee97-07cb8f698939	2025-12-10 21:33:50.201123+08	48lecghj	48lecghj	$2a$11$KepV/lblvm0NWGyao8cHS.5bZJjHKtJ/XuIICXKd8Ggyx7u8HJeHC	inlyo08oym	17599941737	test' OR '1'='1@example.com	User	\N
019b0877-daa0-de06-6a9c-2f88b403f55c	2025-12-10 21:33:51.136481+08	ezow23ky	ezow23ky	$2a$11$JxFmP.yUNEn.UdhnzupJvenuPnqyYtrUKHrUgThcENULt.LRg1oDC	zo0f63piyw	14296095547	test" OR "1"="1@example.com	User	\N
019b0877-de69-be59-fdc7-5a67b9e69c15	2025-12-10 21:33:52.10588+08	xqvkgizg	xqvkgizg	$2a$11$Zc/V83ILVp9mbBq2kPbO7OSlZ7DGXO70nof3XYZHqIwfbwBWE1Q2G	nmir8z5c9i	16754397551	test'; DROP TABLE users; --@example.com	User	\N
019b0877-e1fc-e125-0b72-7e7963f4a91a	2025-12-10 21:33:53.020413+08	1tzl9xdz	1tzl9xdz	$2a$11$FjP/9R.UPmcFUciH4J8dIupBU3E7ulQpSKogFvUALPo.1.h90GTrG	76jrxypg5r	13876069750	testadmin'--@example.com	User	\N
019b0877-e58e-e42d-0b00-d2caf8842c9a	2025-12-10 21:33:53.934716+08	7j9wvldh	7j9wvldh	$2a$11$.H5pwB01MBaposFoGsUk..cbtk.4agvXAKd8/ual53ib5fCx4SRRK	3shfq4k5qa	17395242606	testadmin'/*@example.com	User	\N
019b0877-e931-3054-168b-73afd0f33874	2025-12-10 21:33:54.865758+08	gwrbzba1	gwrbzba1	$2a$11$8vAv/mkOFb4ooyKV0GgHUenYgydNk/YAb5cDWlZV2ICeA75T7B6jy	yfw6obj19d	17973326409	test' UNION SELECT * FROM users--@example.com	User	\N
019b0877-eccc-165b-7e18-b57b3fcac152	2025-12-10 21:33:55.788925+08	pl5zpmki	pl5zpmki	$2a$11$rtO.coV9HL.gwSjd8hNZNuzuVMczKNtEx4w.UeDmk9UvQdOuxQMOC	ai91ebnp1h	15439294398	test' OR 1=1--@example.com	User	\N
019b0877-f078-4e80-1dfe-a367adc27cc9	2025-12-10 21:33:56.728052+08	qj876wri	qj876wri	$2a$11$kTNiaflz95iiG20yJA2zAOd25gSFqPDwSuO9LnUs2YwCM7xE71J1q	lt0lggxl7i	18951018211	test' OR 1=2--@example.com	User	\N
019b0877-f41f-edbb-4b17-9eb8c15cc82c	2025-12-10 21:33:57.663976+08	czooets6	czooets6	$2a$11$U2Oie4jeGjGbWAs2Km5x5ud7etUVpPh8FYMYHR9xPQT3FJ/Cpuwku	gjq7u0lms2	17497427107	test' AND '1'='1@example.com	User	\N
019b0877-f7ac-e5de-33d8-f57e306559e4	2025-12-10 21:33:58.572801+08	a63c36un	a63c36un	$2a$11$gyHArAuEEhIJBan4gFdcMe5QT3Y7GmbKvbEr/2bFljt.mN7uMgFma	f7lc31arxv	15718366785	test' AND '1'='2@example.com	User	\N
019b0877-ff89-1278-2a58-f531183cf129	2025-12-10 21:34:00.585205+08	'; SELECT pg_sleep(10); --	'; SELECT pg_sleep(10); --	$2a$11$hpv6qRDYUIK9SO024rTOmOwxqzFbBcsylipQ5aBcYhT6GWV10ZuU2	lp2h3jjugx	17372991884	\N	User	\N
019b0878-051e-89ec-ddd2-43e788180398	2025-12-10 21:34:02.014491+08	' OR IF(1=1, pg_sleep(5), 0)--	' OR IF(1=1, pg_sleep(5), 0)--	$2a$11$dEY9S9xej8szPpkRTX5nn.K3wjkLs6SWvq3yO8iGJHeVSiP6PVKHC	eab8rsz7tr	14718792865	\N	User	\N
019b0878-0aad-8ce1-9f81-e6abd829ed52	2025-12-10 21:34:03.437859+08	' OR IF(1=2, pg_sleep(5), 0)--	' OR IF(1=2, pg_sleep(5), 0)--	$2a$11$2O8x9AT6FisbW0LSXrsxn.NIR0W6L7elpQ1UnHvNKXfU2o497enPq	bjqi8t6l9u	19567352246	\N	User	\N
019b087b-6ab8-ac0a-02d0-5debaf02f0b1	2025-12-10 21:37:44.632572+08	<script>alert('XSS')</script>ileu21	<script>alert('XSS')</script>ileu21	$2a$11$gpuasCmpWuU3iqArlq9TRet.aDJ12Z39qrMkkqp/klXE975TXJe7K	2024ileu21	14379984937	\N	User	\N
019b087b-6d72-ea0b-9577-43612ed452a3	2025-12-10 21:37:45.330434+08	<SCRIPT>alert('XSS')</SCRIPT>j4ggps	<SCRIPT>alert('XSS')</SCRIPT>j4ggps	$2a$11$r9lo26DJTngJt.XhT.Vpge48eQkgHOfPwBI3FHvh02wHgdagTRe56	2024j4ggps	18498886221	\N	User	\N
019b087b-703e-03e9-3796-a25bd0363278	2025-12-10 21:37:46.046296+08	<ScRiPt>alert('XSS')</ScRiPt>uglus7	<ScRiPt>alert('XSS')</ScRiPt>uglus7	$2a$11$UBBgeUFnuADyDgZZkXKEZuyD3rHOGVAQemOEIsiMDYiCU.JAiziX.	2024uglus7	19356975356	\N	User	\N
019b087b-7305-360e-bc4f-f3338ababc36	2025-12-10 21:37:46.757556+08	<img src=x onerror="alert('XSS')">w4rogz	<img src=x onerror="alert('XSS')">w4rogz	$2a$11$mt2kAM51ZhL8sVmTb/ehluBpJOQXJqnlBUVrVYPHX8Kp1xNUrJuz.	2024w4rogz	17126185626	\N	User	\N
019b087b-75d4-71f9-d169-2176591d8aed	2025-12-10 21:37:47.476729+08	<img src=x onload="alert('XSS')">f8wknw	<img src=x onload="alert('XSS')">f8wknw	$2a$11$ZlNYp15lGeSgSa.qpW/lAOoyQF.ynb2c8V.3VLU8JlCsVRzwL.phu	2024f8wknw	16180065725	\N	User	\N
019b087b-787b-91cb-9062-613c665ad960	2025-12-10 21:37:48.15529+08	<div onclick="alert('XSS')">点击我</div>8axee2	<div onclick="alert('XSS')">点击我</div>8axee2	$2a$11$QQ9nNFlKzwRZI5SxeNaqs.5HaAF.y8LxJjHTPICq1j3t7qb9oFDsq	20248axee2	19470428140	\N	User	\N
019b087b-7b38-4bc1-f998-b9efe809d814	2025-12-10 21:37:48.856182+08	<body onload="alert('XSS')">d8s0sp	<body onload="alert('XSS')">d8s0sp	$2a$11$eeNgCqkXn6xdo74jLmDciunUrxc2Of2aZmEW9kxf7.fGLNSa0Edjq	2024d8s0sp	14659828595	\N	User	\N
019b087b-7e04-f4bb-d4f7-816d71301302	2025-12-10 21:37:49.572119+08	<input onfocus="alert('XSS')" autofocus>3gg4d8	<input onfocus="alert('XSS')" autofocus>3gg4d8	$2a$11$vwN72oVl9cOvSJi5cdAbsecy.c.SOYylkUYxwm3zuPZPkkJKHxU3i	20243gg4d8	19418116007	\N	User	\N
019b087b-80a4-8542-85fb-9a51ee584135	2025-12-10 21:37:50.244012+08	<svg onload="alert('XSS')">zdtzjx	<svg onload="alert('XSS')">zdtzjx	$2a$11$P1Ydmk10EoD1WIA6dVlKb.4yomIIGaGy14S4onIVoO0V2VRAdQRSS	2024zdtzjx	14268403610	\N	User	\N
019b087b-8342-994a-1a80-595c56546846	2025-12-10 21:37:50.914025+08	javascript:alert('XSS')m5zscl	javascript:alert('XSS')m5zscl	$2a$11$m1JXi.EtuT8SnVVLNsX.qemWPw6hvfJ3Y4citHrcYbr/4Vj3hsnY.	2024m5zscl	15991094420	\N	User	\N
019b087b-85d0-dcc1-4b0f-8f4c9f97f34f	2025-12-10 21:37:51.56868+08	javascript&#58;alert('XSS')s3ppct	javascript&#58;alert('XSS')s3ppct	$2a$11$sIuXocXOWObT5ti4s00L9.U1Kzjx4zA5sfMXISlg4Vm4Ah0SWxWai	2024s3ppct	17441773918	\N	User	\N
019b087b-8874-8b5e-867a-0605ae37ad08	2025-12-10 21:37:52.244715+08	<iframe src="javascript:alert('XSS')"></iframe>73imgl	<iframe src="javascript:alert('XSS')"></iframe>73imgl	$2a$11$Q/2FPVqEJZdZnW/jmZ8zBuN1VcUnVRKPOy.fwoXeOJSw6dMI12fYy	202473imgl	14299305780	\N	User	\N
019b087b-8b21-9e2e-aeec-729e39ae8287	2025-12-10 21:37:52.929477+08	&#60;script&#62;alert('XSS')&#60;&#47;script&#62;lazwjw	&#60;script&#62;alert('XSS')&#60;&#47;script&#62;lazwjw	$2a$11$4hzQqnLyOXZe/PD0cdOZH.6PCCg0bY1tFoN7hrbrY6OOyEP0.5yOu	2024lazwjw	19193398643	\N	User	\N
019b087b-8dce-1a24-0e1c-1c36be67cd52	2025-12-10 21:37:53.614863+08	\\u003cscript\\u003ealert('XSS')\\u003c/script\\u003euofj9r	\\u003cscript\\u003ealert('XSS')\\u003c/script\\u003euofj9r	$2a$11$S5TnvszMZI71kMDD9AokTeLjuUGd5dR9Z6qeLv4dEGynSL/VTdJHK	2024uofj9r	13949030785	\N	User	\N
019b087b-907a-f339-9325-3017a602843d	2025-12-10 21:37:54.298834+08	%3Cscript%3Ealert('XSS')%3C/script%3Eds3ap0	%3Cscript%3Ealert('XSS')%3C/script%3Eds3ap0	$2a$11$B0EfomAyPCtQsfum43OXiubwYdpGDdkqLIwPthnfazmB6TSsyEq4e	2024ds3ap0	19630052333	\N	User	\N
019b087b-9329-657d-0262-8e7f2148b202	2025-12-10 21:37:54.985059+08	testuser_vaadba	testuser_vaadba	$2a$11$FIXUppZfgMhxjRkYfWxcourz75oYWo2ShdBrcXbTHM9cp9xW31yz6	<script>alert('XSS')</script>vaadba	13615061924	\N	User	\N
019b087b-95ed-0312-0ad0-918bf1682406	2025-12-10 21:37:55.693385+08	testuser_565scu	testuser_565scu	$2a$11$Md77y4UA7R29dEgEPpUxGekMRtqTun7C4q/yCUgdFbgT8r1syRUHi	<SCRIPT>alert('XSS')</SCRIPT>565scu	15664259365	\N	User	\N
019b087b-98ad-b33c-815b-fe62ec733186	2025-12-10 21:37:56.397519+08	testuser_ifp8ua	testuser_ifp8ua	$2a$11$HJhzFtNyv6j6dwkvFlMDkeY3aznpYy3jkhBwBh4FSL9G6sj4qKFe6	<ScRiPt>alert('XSS')</ScRiPt>ifp8ua	15724935638	\N	User	\N
019b087b-9b6f-cc72-c6e6-ca6ee19d59fd	2025-12-10 21:37:57.103575+08	testuser_hgrdj3	testuser_hgrdj3	$2a$11$.jBlNv3Gq8723NPxX0M/bOqyLUjF/gcbSudhvfgIJkwzZYfJAKjl2	<img src=x onerror="alert('XSS')">hgrdj3	16102757934	\N	User	\N
019b087b-9e41-0459-6993-efa6dbe02b8e	2025-12-10 21:37:57.825274+08	testuser_oryynz	testuser_oryynz	$2a$11$C3Owk6M1knO3nkxOjRRqYOFpBaUYeasVdI5uulWD7ooUFIPm2iRki	<img src=x onload="alert('XSS')">oryynz	17457333381	\N	User	\N
019b087b-a113-e069-ac2f-16fd138d1a19	2025-12-10 21:37:58.547369+08	testuser_apgnva	testuser_apgnva	$2a$11$Fl.dibT01gyz61o3.enw4eDbxWOZfFjZtzq7U3TrPfTJIwh74fzLW	<div onclick="alert('XSS')">点击我</div>apgnva	18224082576	\N	User	\N
019b087b-a3e4-50f9-78b2-68eac1dc4c9f	2025-12-10 21:37:59.268031+08	testuser_y100pa	testuser_y100pa	$2a$11$6kvfR4SLzrdZKMDz3fZF5eoSHSH8fOIGimXs5tWODWB/9e94N2bga	<body onload="alert('XSS')">y100pa	13914879717	\N	User	\N
019b087b-a6a8-daa0-bcb3-b0ca92188ecc	2025-12-10 21:37:59.976773+08	testuser_7nve28	testuser_7nve28	$2a$11$X8PBpo4P8adxbMACivnrhOl3KBO3dsygD.xLMD2emNDF1qcdnuBKO	<input onfocus="alert('XSS')" autofocus>7nve28	13222653095	\N	User	\N
019b087b-a970-0a19-440a-d04c94f83b68	2025-12-10 21:38:00.688426+08	testuser_1542cw	testuser_1542cw	$2a$11$2W.oC65EwGoFAmPdqtlmr.OZcv6Yvl9on8WkR.hiBH0V4zsDvO7h6	<svg onload="alert('XSS')">1542cw	18614110243	\N	User	\N
019b087b-ac07-bf96-db43-60ef448c7ba6	2025-12-10 21:38:01.351393+08	testuser_xju2vm	testuser_xju2vm	$2a$11$fZZQGssYKN6q63DQejlKa.03lcQDHBiqEz6eQoRcqjT1QJEs3xtCC	javascript:alert('XSS')xju2vm	18213530223	\N	User	\N
019b087b-aea6-d042-c519-a9f4274e108b	2025-12-10 21:38:02.022687+08	testuser_ogb3cy	testuser_ogb3cy	$2a$11$pS2VrenWaXYlNMZyaMtdw.17U6ZUwv74mXoRBi0i51wOsHQPOgbi.	2024ogb3cy	19643055907	test<script>alert('XSS')</script>@example.com	User	\N
019b087b-b141-b8e9-2235-1bde2bd51998	2025-12-10 21:38:02.689737+08	testuser_wp04l0	testuser_wp04l0	$2a$11$eRFLQ/7B/Er51bEGZcbsG.yHnRYWjsj35BSkNfS4HtyFnUcDiTFR6	2024wp04l0	19991595233	test<SCRIPT>alert('XSS')</SCRIPT>@example.com	User	\N
019b087b-b3ea-fab4-9ec3-6d00e7548e50	2025-12-10 21:38:03.37045+08	testuser_dl6s3f	testuser_dl6s3f	$2a$11$2M3oIMl9QygWc9xpuYcf9e53TU/S7B8m/7aBbuJ84s7Go6qzP4gD2	2024dl6s3f	14971825746	test<ScRiPt>alert('XSS')</ScRiPt>@example.com	User	\N
019b087b-b6a4-984a-9b5f-b595916535c0	2025-12-10 21:38:04.068508+08	testuser_czoqu9	testuser_czoqu9	$2a$11$m/KMDPJXkUL7jnivcUdJUOPLbuoBrGznvaDJqCG3g9v0GvXC9IcAq	2024czoqu9	13752794955	test<img src=x onerror="alert('XSS')">@example.com	User	\N
019b087b-b955-521f-09af-6c82a866caaf	2025-12-10 21:38:04.757932+08	testuser_wdi9lk	testuser_wdi9lk	$2a$11$CelUqMqspv.MSju48g41mOhLhyjM1GJ0/F.kZoRlj11ZQIyVcwHc.	2024wdi9lk	15956152446	test<img src=x onload="alert('XSS')">@example.com	User	\N
019b087b-bc12-f064-fef1-f8dbf99941c8	2025-12-10 21:38:05.458258+08	testuser_nvbfdl	testuser_nvbfdl	$2a$11$Hsqd1IKC32OdElvrPoBDsexzipb5/EFgvHH9DSNpWjpzIz7iHSm3C	2024nvbfdl	19847692889	test<div onclick="alert('XSS')">点击我</div>@example.com	User	\N
019b087b-beed-f9ce-2088-461c88c42294	2025-12-10 21:38:06.189006+08	testuser_smbvgn	testuser_smbvgn	$2a$11$N4rfDMyT7cvOgrQ9SdNrrOkQ4VTfGd8nDiihinaiB8RZklUJLC4ey	2024smbvgn	17660097612	test<body onload="alert('XSS')">@example.com	User	\N
019b087b-c1c6-8c0e-28bc-7152c6e6f963	2025-12-10 21:38:06.918367+08	testuser_q0wevh	testuser_q0wevh	$2a$11$3gSU2lCbdRk7otrxXTfBReleMTYjQTSDPrIMmohEeTBB8F5Ib6HnO	2024q0wevh	19397428224	test<input onfocus="alert('XSS')" autofocus>@example.com	User	\N
019b087b-c498-fcff-7c33-ed21c8b4db3d	2025-12-10 21:38:07.640319+08	testuser_oa26as	testuser_oa26as	$2a$11$T0u0yA/rbzTKrE0I7wFdC.wEplthtfyNG9qtsoawXWEMPGokuZRtq	2024oa26as	17778963065	test<svg onload="alert('XSS')">@example.com	User	\N
019b087b-c768-3a37-6fe8-1e028988f3e4	2025-12-10 21:38:08.360753+08	testuser_sqo4yd	testuser_sqo4yd	$2a$11$JeRIo6iBUDL.zneMeEbXieLBZNvAEcd/shjzuxv2A311AiJEr5yCq	2024sqo4yd	14476962477	testjavascript:alert('XSS')@example.com	User	\N
019b087b-db6c-8318-2fcc-80423dfb935b	2025-12-10 21:38:13.484682+08	testxss<script>alert(1)</script>123	testxss<script>alert(1)</script>123	$2a$11$kvcI91CFShdeYed5vg4.dO2.L/kaxeCyUhIeHxyzegkY1wV.ABXJ6	2024001001	13812345678	\N	User	\N
019b2b4e-19e2-e760-6e20-08392ad91f5c	2025-12-17 15:54:57.378717+08	AFunDog	AFunDog	$2a$11$r7Wh6RumfdSOur/XdFcBxutalFGwYiaRFeUwCclXbPVbKjZWA7oAC	236001302	13986748732	\N	Admin	\N
019b0c10-3675-9886-7dce-993e16d05baf	2025-12-11 14:19:07.764773+08	aord1	aord1	$2a$11$eLIxuYQVtN29Nf2lNyNuG.vC3nOUzZJv6936VGJb0XtOn5943d1PG	234001113	14511111113	122455@123.com	User	\N
019b02b2-5a66-1a39-409c-3cc84569706d	2025-12-09 18:40:01.638211+08	admin	admin	$2a$11$NPmEUDEst1olao8..eOIq.wn03/3Z1jApValKlMsat7A1SBgeKS2W	236006667	16666666666	\N	Admin	\N
019b0c37-440c-bd44-b05d-2aa925e02bc0	2025-12-11 15:01:47.148491+08	testuser011	testuser011	$2a$11$UaYCk0F87/f9judH/i03ieqt9B8gIAxTfHy/G/dPAbmiYckFQWcIa	236013821	17843030316	2590181236@qq.com	User	\N
019b2ff0-76c0-be59-fdd2-678ca2438a22	2025-12-18 13:30:46.848706+08	testuser09999	testuser09999	$2a$11$3cybhTLAcj10UU9Krys3EulyP1CTbr6eLooqikqFApGNh8J3jYu0C	2140533921	17843570218	\N	User	\N
019b0c72-eac9-c9d8-97cf-b1a97f6f73f3	2025-12-11 16:06:56.457322+08	admin2	admin2	$2a$11$WdTM9GxntquIo3uZurWj6ePBRUO.PTMA6s8m3w.5TFefXUxDYeGoW	236009999	16699999999	654321@123.com	Admin	\N
019b2ffa-2f24-56d0-ab5b-b1baea46b499	2025-12-18 13:41:23.876239+08	A_鞠俊材	A_鞠俊材	$2a$11$isNbD2oBqNAZybOa2zZNt.V1c4FP0F4BUJGDAlC8K/IOiEATJJ4Ou	257003921	17845980218	\N	User	\N
019b0c2c-7511-3b4a-90f2-05ef3f1a563f	2025-12-11 14:49:58.801567+08	Mr.Zhao2	Mr.Zhao2	$2a$11$JdMXQOQt/MmPiitMWUA6E.Io.Gu5m0DND2numv.P52INY7ajlpct6	236004000	14566661111	\N	User	\N
019b0887-0e14-97cb-9739-66ead959acf1	2025-12-10 21:50:27.348351+08	DaoDaoYu	dddyyy	$2a$11$u/kBOma5ElKgZKikSiwDhe70BXiiVWa5roo2eHNXcLTDz5ie6pTDu	23600000	18658660001	1145141919810@qq.com	Admin	\N
019b3005-b548-5d4c-ecfa-3fa34aff0d73	2025-12-18 13:53:59.112767+08	testuser154	testuser154	$2a$11$YEBngFbIOzD/rTmKwqEAZu50BvZVebe1pYF.t8X8EfTXFDQ2d8fg6	256053921	17878787878	\N	User	\N
019b3008-cc6b-aaed-b6ad-6588b2d4c131	2025-12-18 13:57:21.643822+08	testuser156	testuser156	$2a$11$/s9Z6U8XG8qib/pCF9mUF.GUcrVuqAAe8FY65z0KM9przzoI8Ovu2	254003921	17876767676	\N	Admin	\N
\.


--
-- Data for Name: violations; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.violations (id, user_id, state, type, content, booking_id, create_time) FROM stdin;
019b2f3f-0433-bce6-e091-0842b1dfdd2a	019b02b2-5a66-1a39-409c-3cc84569706d	Violation	超时	未在规定时间签到	019b2f2a-703b-5d87-c96f-be081a2a7b4d	2025-12-18 10:16:57.653327+08
019b2fea-ee77-f0e0-3ca0-051a67c9c5b1	019b02b2-5a66-1a39-409c-3cc84569706d	Violation	超时	未在规定时间签退	019b2f2c-9214-0952-a297-f649f9b0b87f	2025-12-18 13:24:44.28161+08
019b304f-e450-04af-922b-f9afc0b1403c	019b02b2-5a66-1a39-409c-3cc84569706d	Violation	超时	未在规定时间签到	019b3016-afa4-43d3-7a23-14c9010e0c55	2025-12-18 15:15:00.817203+08
\.


--
-- Name: audit_logs audit_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_pkey PRIMARY KEY (id);


--
-- Name: blacklists blacklists_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.blacklists
    ADD CONSTRAINT blacklists_pkey PRIMARY KEY (id);


--
-- Name: bookings bookings_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_pkey PRIMARY KEY (id);


--
-- Name: complaints complaints_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.complaints
    ADD CONSTRAINT complaints_pkey PRIMARY KEY (id);


--
-- Name: rooms rooms_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rooms
    ADD CONSTRAINT rooms_pkey PRIMARY KEY (id);


--
-- Name: seats seats_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.seats
    ADD CONSTRAINT seats_pkey PRIMARY KEY (id);


--
-- Name: users users_campus_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_campus_id_key UNIQUE (campus_id);


--
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- Name: users users_phone_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_phone_key UNIQUE (phone);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: users users_user_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_user_name_key UNIQUE (user_name);


--
-- Name: violations violations_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.violations
    ADD CONSTRAINT violations_pkey PRIMARY KEY (id);


--
-- Name: bookings bookings_audit_log; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER bookings_audit_log AFTER INSERT OR DELETE OR UPDATE ON public.bookings FOR EACH ROW EXECUTE FUNCTION public.audit_log();


--
-- Name: complaints complaints_audit_log; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER complaints_audit_log AFTER INSERT OR DELETE OR UPDATE ON public.complaints FOR EACH ROW EXECUTE FUNCTION public.audit_log();


--
-- Name: bookings on_data_change; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER on_data_change AFTER INSERT OR DELETE OR UPDATE ON public.bookings FOR EACH ROW EXECUTE FUNCTION public.notify_data_change();


--
-- Name: rooms rooms_audit_log; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER rooms_audit_log AFTER INSERT OR DELETE OR UPDATE ON public.rooms FOR EACH ROW EXECUTE FUNCTION public.audit_log();


--
-- Name: seats seats_audit_log; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER seats_audit_log AFTER INSERT OR DELETE OR UPDATE ON public.seats FOR EACH ROW EXECUTE FUNCTION public.audit_log();


--
-- Name: users users_audit_log; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER users_audit_log AFTER INSERT OR DELETE OR UPDATE ON public.users FOR EACH ROW EXECUTE FUNCTION public.audit_log();


--
-- Name: violations violations_audit_log; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER violations_audit_log AFTER INSERT OR DELETE OR UPDATE ON public.violations FOR EACH ROW EXECUTE FUNCTION public.audit_log();


--
-- Name: blacklists blacklists_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.blacklists
    ADD CONSTRAINT blacklists_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: bookings bookings_seat_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_seat_id_fkey FOREIGN KEY (seat_id) REFERENCES public.seats(id) ON UPDATE CASCADE ON DELETE CASCADE NOT VALID;


--
-- Name: bookings bookings_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON UPDATE CASCADE ON DELETE CASCADE NOT VALID;


--
-- Name: complaints complaints_handle_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.complaints
    ADD CONSTRAINT complaints_handle_user_id_fkey FOREIGN KEY (handle_user_id) REFERENCES public.users(id) ON UPDATE CASCADE ON DELETE CASCADE NOT VALID;


--
-- Name: complaints complaints_seat_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.complaints
    ADD CONSTRAINT complaints_seat_id_fkey FOREIGN KEY (seat_id) REFERENCES public.seats(id) ON UPDATE CASCADE ON DELETE CASCADE NOT VALID;


--
-- Name: complaints complaints_send_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.complaints
    ADD CONSTRAINT complaints_send_user_id_fkey FOREIGN KEY (send_user_id) REFERENCES public.users(id) ON UPDATE CASCADE ON DELETE CASCADE NOT VALID;


--
-- Name: seats seats_roomId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.seats
    ADD CONSTRAINT "seats_roomId_fkey" FOREIGN KEY (room_id) REFERENCES public.rooms(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- Name: violations violations_booking_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.violations
    ADD CONSTRAINT violations_booking_id_fkey FOREIGN KEY (booking_id) REFERENCES public.bookings(id) NOT VALID;


--
-- Name: violations violations_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.violations
    ADD CONSTRAINT violations_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON UPDATE CASCADE ON DELETE CASCADE NOT VALID;


--
-- PostgreSQL database dump complete
--

\unrestrict Y8gbW4Z1tel80SuJGmVL2cNMahMdU8EzGoA1TuGy2a5KK1qGH2XOVTZSCmNheEV

