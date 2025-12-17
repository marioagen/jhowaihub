<template>
    <ModalComponent id="tenantModal" ref="TenantModal">
        <template #header>
            <div class="modal-header">
                <h6 class="modal-title"> {{ $t("login.selectTenant") }} </h6>
                <button class="btn-close" data-bs-dismiss="modal" @click="close" />
            </div>
        </template>

        <template #body>
            <div class="modal-body">
                <label>{{ $t("login.selectTenant") }}</label>
                <select v-model="tenant" class="form-select form-select-sm">
                    <option v-for="item in tenants" :key="item" :value="item">
                        {{ item }}
                    </option>
                </select>
            </div>
        </template>

        <template #footer>
            <div class="modal-footer">
                <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="close">
                    {{ $t("common.cancel") }}
                </button>
                <button class="btn btn-primary btn-sm" @click="continueLogin">
                    {{ $t("login.continue") }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>

<script>
import ModalComponent from '@/components/global/ModalComponent.vue';
export default {
    components: {
        ModalComponent
    },
    emits: ["continueLogin"],
    props: {
        tenants: {
            type: Array,
            required: true
        },
        typeLogin: {
            type: String,
            required: true
        }
    },
    data: () => ({
        tenant:  ""
    }),
    methods: {
        open() {            
            this.$refs.TenantModal.open();
        },
        close() {
            this.$refs.TenantModal.close();
        },
        continueLogin() {
            this.$emit("continueLogin", this.tenant, this.typeLogin);
            this.close();
        }
    }
}
</script>