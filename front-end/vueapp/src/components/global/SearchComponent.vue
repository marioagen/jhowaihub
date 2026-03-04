<template>
    <div class="row">
        <div class="col">
            <div class="input-group">
                <span class="input-group-text border-end-0">
                    <LucideIcon icon="Search" :size="16" />
                </span>
                <input
                    id="InputSearch"
                    type="text"
                    class="form-control form-control-sm border-start-0 custom-input"
                    :class="{ 'border-end-0': showCleanBtn }"
                    ref="searchInpt"
                    v-model="searchInput"
                    :placeholder="entity.placeholderInput"
                    @keydown.enter="search(1, 'search')"
                    @keydown.delete="search(1, 'search')"
                />
                <span v-if="showCleanBtn" class="input-group-text border-start-0" @click="cleanBtn">
                    <LucideIcon icon="X" :size="16" />
                </span>
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: "SearchBar",
        props: {
            entity: {
                required: true,
                type: Object,
                default: () => {},
            },
            resetInput: {
                required: true,
                type: Boolean,
                default: false,
            },
        },
        data() {
            return {
                title: "Component SearchBar",
                searchInput: "",
            };
        },
        watch: {
            resetInput() {
                this.searchInput = "";
            },
        },
        methods: {
            search(page, type) {
                setTimeout(() => {
                    if (this.searchInput.length > 0 || (!isNaN(this.searchInput) && parseInt(this.searchInput) > 0)) {
                        this.$emit("search", { search: this.searchInput, page: page, type: type });
                    } else {
                        this.$emit("search", { search: "", page: page, type: type });
                    }
                }, 100);
            },
            action() {
                this.$emit("action", this.searchInput);
            },
            cleanBtn() {
                this.searchInput = "";
                this.$emit("clean", { search: "" });
            },
        },
        computed: {
            showCleanBtn() {
                return this.searchInput !== "";
            },
        },
        mounted() {
            this.$refs.searchInpt.focus();
        },
    };
</script>

<style scooped>
    .custom-input {
        font-size: 12px;
    }

    .custom-input::placeholder {
        font-size: 12px;
        color: #999;
    }
</style>
