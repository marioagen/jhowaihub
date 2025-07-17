<template>
    <div class="row">
        <div class="col">
            <div class="input-group">
                <span class="input-group-text border-end-0 bg-white">
                    <LucideIcon icon="Search" size="16" />
                </span>
                <input 
                    id="InputSearch" 
                    type="text" 
                    class="form-control form-control-sm border-start-0"
                    :class="{ 'border-end-0': showCleanBtn }"
                    ref="searchInpt" 
                    v-model="searchInput" 
                    :placeholder="entity.placeholderInput" 
                    @keydown.enter="search(1, 'search')" 
                    @keydown.delete="search(1, 'search')"
                >
                <span 
                    v-if="showCleanBtn"
                    class="input-group-text border-start-0 bg-white"
                    @click="cleanBtn"
                >
                    <LucideIcon icon="X" size="16" />
                </span>
            </div>
        </div>

        <template 
            v-if="entity.screen !== 'user' && entity.screen !== 'team'"
        >
            <div 
                v-if="entity.screen != 'document'"
                class="col-auto content-center" 
            >
                <div class="mb-2">
                    <a 
                        v-if="searchInput.length >= 3"
                        class="btn btn-primary" 
                        :title="entity.labelButton" 
                        @click="action" 
                    >
                        {{ upperFormat(entity.labelButton) }}
                    </a>
                    <a 
                        v-else
                        class="btn btn-secondary" 
                        :title="$t('labelNotAllowed')" 
                    >
                        {{ upperFormat(entity.labelButton) }}
                    </a>
                </div>
            </div>
            
            <div class="col-auto content-center" v-else>
                <div class="mb-2">
                    <a class="btn btn-primary"
                       :title="$t('labelNewDocument')"
                       @click="action">
                        {{ $t('labelNewDocument') }}
                    </a>
                </div>
            </div>
        </template>
    </div>
</template>

<script>
    export default {
        name: "SearchBar",
        props: {
            entity: {
                required: true,
                type: Object,
                default: () => {}
            },
            resetInput: {
                required: true,
                type: Boolean,
                default: false
            },
        },
        data() {
            return {
                title: "Component SearchBar",
                searchInput: "",
            }
        },
        watch: {
            resetInput () {
                this.searchInput = "";
            }
        },
        methods: {
            search(page, type) {
                setTimeout(() => {
                    if (this.searchInput.length > 0 || (!isNaN(this.searchInput) && parseInt(this.searchInput) > 0)) {
                        this.$emit('search', { search: this.searchInput, page: page, type: type });
                    }
                    else {
                        this.$emit('search', { search: "", page: page, type: type });
                    }
                }, 100);
            },
            action() {
                this.$emit('action', this.searchInput);
            },
            upperFormat(str) {
                return str.toUpperCase();
            },
            cleanBtn() {
                this.searchInput = "";
                this.$emit('clean', { search: "" });
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
    }
</script>