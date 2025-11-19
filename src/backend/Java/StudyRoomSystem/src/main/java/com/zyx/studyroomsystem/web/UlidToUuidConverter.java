package com.zyx.studyroomsystem.web;

import de.huxhorn.sulky.ulid.ULID;
import java.util.UUID;

public class UlidToUuidConverter {
    private static final ULID ulidGen = new ULID();

    // 生成 ULID 并转换为 UUID
    public static UUID generateUuidFromUlid() {
        ULID.Value ulidValue = ulidGen.nextValue();
        return new UUID(ulidValue.getMostSignificantBits(), ulidValue.getLeastSignificantBits());
    }
}

