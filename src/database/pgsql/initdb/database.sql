--
-- PostgreSQL database dump
--

\restrict DGr0BUs2AQRdLfiPbx5NNEQQYCRU2XjKkw8sKvV8RjN7VkBd2fpNryUNsQouNZy

-- Dumped from database version 16.9
-- Dumped by pg_dump version 18.0

-- Started on 2025-12-09 16:53:09

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
-- TOC entry 4 (class 2615 OID 2200)
-- Name: public; Type: SCHEMA; Schema: -; Owner: pg_database_owner
--

CREATE SCHEMA public;


ALTER SCHEMA public OWNER TO pg_database_owner;

--
-- TOC entry 4890 (class 0 OID 0)
-- Dependencies: 4
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: pg_database_owner
--

COMMENT ON SCHEMA public IS 'standard public schema';


--
-- TOC entry 221 (class 1255 OID 17833)
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
-- TOC entry 218 (class 1259 OID 17811)
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
-- TOC entry 220 (class 1259 OID 17871)
-- Name: complaints; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.complaints (
    id uuid NOT NULL,
    send_user_id uuid NOT NULL,
    receive_user_id uuid NOT NULL,
    state text NOT NULL,
    type text NOT NULL,
    content text NOT NULL,
    create_time timestamp with time zone NOT NULL,
    handle_time timestamp with time zone,
    handle_user_id uuid
);


ALTER TABLE public.complaints OWNER TO postgres;

--
-- TOC entry 216 (class 1259 OID 17794)
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
-- TOC entry 217 (class 1259 OID 17801)
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
-- TOC entry 215 (class 1259 OID 17787)
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
-- TOC entry 219 (class 1259 OID 17858)
-- Name: violations; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.violations (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    state text NOT NULL,
    type text NOT NULL,
    content text NOT NULL
);


ALTER TABLE public.violations OWNER TO postgres;

--
-- TOC entry 4882 (class 0 OID 17811)
-- Dependencies: 218
-- Data for Name: bookings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.bookings (id, user_id, seat_id, create_time, start_time, end_time, check_in_time, check_out_time, state) FROM stdin;
019a4cd3-4750-e38a-9e5d-8f824d042851	019a1b60-de17-ead2-3902-26fb51efc5bf	019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	2025-11-04 11:05:06.133175+08	2025-11-05 04:25:00+08	2025-11-05 05:40:00+08	\N	\N	Canceled
019a4cd3-4750-e38a-9e5d-8f824d042852	019a1b60-de17-ead2-3902-26fb51efc5bf	019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	2025-11-04 11:05:06.133175+08	2025-11-05 04:25:00+08	2025-11-05 05:40:00+08	\N	\N	Canceled
019a7ba6-0a1a-ccd3-56d1-e7f6b4da47f0	019a1b60-de17-ead2-3902-26fb51efc5bf	019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	2025-11-13 13:17:50.496151+08	2025-11-19 06:30:00+08	2025-11-19 11:40:00+08	\N	\N	Canceled
019afbd2-28db-c438-69bd-021b3fd06b63	019a1b60-de17-ead2-3902-26fb51efc5bf	019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	2025-12-08 10:37:25.60505+08	2025-12-08 22:14:14.873824+08	2025-12-08 23:14:14.873824+08	\N	\N	Booking
019afbd6-8723-d5e2-11ff-9ff5fab254c2	019a1b60-de17-ead2-3902-26fb51efc5bf	019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	2025-12-08 10:42:11.882352+08	2025-12-08 20:22:54.336911+08	2025-12-08 21:22:54.336911+08	\N	\N	Booking
019b0121-61b5-3455-d340-09fd8f8bf6bd	019b00ec-6869-76e3-5ad7-f4fbea686fc4	019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	2025-12-09 11:22:03.581858+08	2025-12-10 00:42:50.001222+08	2025-12-10 03:42:50.001222+08	\N	\N	Booking
\.


--
-- TOC entry 4884 (class 0 OID 17871)
-- Dependencies: 220
-- Data for Name: complaints; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.complaints (id, send_user_id, receive_user_id, state, type, content, create_time, handle_time, handle_user_id) FROM stdin;
\.


--
-- TOC entry 4880 (class 0 OID 17794)
-- Dependencies: 216
-- Data for Name: rooms; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.rooms (id, name, open_time, close_time, rows, cols) FROM stdin;
019a1e1a-0802-6976-c80c-60c92a59c0ad	101	07:00:00	22:00:00	5	8
019a204d-2693-b609-dfd8-90af43016dba	102	06:50:00	21:00:00	4	7
\.


--
-- TOC entry 4881 (class 0 OID 17801)
-- Dependencies: 217
-- Data for Name: seats; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.seats (id, room_id, "row", col) FROM stdin;
019a1e1b-2c7a-fcdb-6ebe-8e70904583a8	019a1e1a-0802-6976-c80c-60c92a59c0ad	1	2
019a1e1b-c9be-aa61-f0c9-7909ca47fa43	019a1e1a-0802-6976-c80c-60c92a59c0ad	3	3
019a2531-961d-f2a0-9e98-cd0038609c9c	019a1e1a-0802-6976-c80c-60c92a59c0ad	3	2
\.


--
-- TOC entry 4879 (class 0 OID 17787)
-- Dependencies: 215
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, create_time, user_name, display_name, password, campus_id, phone, email, role, avatar) FROM stdin;
019a1b60-de17-ead2-3902-26fb51efc5bf	2025-10-25 20:38:44.50294+08	zengkun	ZengKun	$2a$11$whcOhV3mebB7dS/b1zad3eyEasRM0gRR3KtbnELfNU5X9DLpAWJAi	236001209	15336567166	1489782679@qq.com	Admin	/api/v1/file/019a7aa9-27e7-2607-6c94-d2a3946fd8d6.png
019b00ec-6869-76e3-5ad7-f4fbea686fc4	2025-12-09 10:24:11.88121+08	zengkun2	zengkun2	$2a$11$W9wEDOP572wojWDtfQ1UuuEuXhB.5l0j.Kb4WrD8ZQL3c7Qwa0zf6	236001208	123456	1489782672@qq.com	User	/api/v1/file/019a7aa9-27e7-2607-6c94-d2a3946fd8d6.png
\.


--
-- TOC entry 4883 (class 0 OID 17858)
-- Dependencies: 219
-- Data for Name: violations; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.violations (id, user_id, state, type, content) FROM stdin;
\.


--
-- TOC entry 4723 (class 2606 OID 17815)
-- Name: bookings bookings_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_pkey PRIMARY KEY (id);


--
-- TOC entry 4727 (class 2606 OID 17877)
-- Name: complaints complaints_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.complaints
    ADD CONSTRAINT complaints_pkey PRIMARY KEY (id);


--
-- TOC entry 4719 (class 2606 OID 17798)
-- Name: rooms rooms_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.rooms
    ADD CONSTRAINT rooms_pkey PRIMARY KEY (id);


--
-- TOC entry 4721 (class 2606 OID 17805)
-- Name: seats seats_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.seats
    ADD CONSTRAINT seats_pkey PRIMARY KEY (id);


--
-- TOC entry 4709 (class 2606 OID 17845)
-- Name: users users_campus_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_campus_id_key UNIQUE (campus_id);


--
-- TOC entry 4711 (class 2606 OID 17896)
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- TOC entry 4713 (class 2606 OID 17847)
-- Name: users users_phone_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_phone_key UNIQUE (phone);


--
-- TOC entry 4715 (class 2606 OID 17793)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- TOC entry 4717 (class 2606 OID 17843)
-- Name: users users_user_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_user_name_key UNIQUE (user_name);


--
-- TOC entry 4725 (class 2606 OID 17864)
-- Name: violations violations_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.violations
    ADD CONSTRAINT violations_pkey PRIMARY KEY (id);


--
-- TOC entry 4735 (class 2620 OID 17835)
-- Name: bookings on_data_change; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER on_data_change AFTER INSERT OR DELETE OR UPDATE ON public.bookings FOR EACH ROW EXECUTE FUNCTION public.notify_data_change();


--
-- TOC entry 4729 (class 2606 OID 17853)
-- Name: bookings bookings_seat_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_seat_id_fkey FOREIGN KEY (seat_id) REFERENCES public.seats(id) NOT VALID;


--
-- TOC entry 4730 (class 2606 OID 17848)
-- Name: bookings bookings_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bookings
    ADD CONSTRAINT bookings_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) NOT VALID;


--
-- TOC entry 4732 (class 2606 OID 17888)
-- Name: complaints complaints_handle_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.complaints
    ADD CONSTRAINT complaints_handle_user_id_fkey FOREIGN KEY (handle_user_id) REFERENCES public.users(id) NOT VALID;


--
-- TOC entry 4733 (class 2606 OID 17878)
-- Name: complaints complaints_send_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.complaints
    ADD CONSTRAINT complaints_send_user_id_fkey FOREIGN KEY (send_user_id) REFERENCES public.users(id) NOT VALID;


--
-- TOC entry 4734 (class 2606 OID 17883)
-- Name: complaints complaints_send_user_id_fkey1; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.complaints
    ADD CONSTRAINT complaints_send_user_id_fkey1 FOREIGN KEY (send_user_id) REFERENCES public.users(id) NOT VALID;


--
-- TOC entry 4728 (class 2606 OID 17806)
-- Name: seats seats_roomId_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.seats
    ADD CONSTRAINT "seats_roomId_fkey" FOREIGN KEY (room_id) REFERENCES public.rooms(id) ON UPDATE CASCADE ON DELETE CASCADE;


--
-- TOC entry 4731 (class 2606 OID 17865)
-- Name: violations violations_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.violations
    ADD CONSTRAINT violations_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


-- Completed on 2025-12-09 16:53:10

--
-- PostgreSQL database dump complete
--

\unrestrict DGr0BUs2AQRdLfiPbx5NNEQQYCRU2XjKkw8sKvV8RjN7VkBd2fpNryUNsQouNZy

