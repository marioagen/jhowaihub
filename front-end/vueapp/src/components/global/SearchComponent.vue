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
                    ref="searchInpt" 
                    v-model="searchInput" 
                    :placeholder="entity.placeholderInput" 
                    @keydown.enter="search(1, 'search')" 
                    @keydown.delete="search(1, 'search')"
                >
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
                default: {}
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
            resetInput: function (val) {
                this.searchInput = "";
            }
        },
        methods: {
            search: function (page, type) {
                let self = this;
                setTimeout(function () {
                    if (self.searchInput.length > 0 || (!isNaN(self.searchInput) && parseInt(self.searchInput) > 0)) {
                        self.$emit('search', { search: self.searchInput, page: page, type: type });
                    }
                    else {
                        self.$emit('search', { search: "", page: page, type: type });
                    }
                }, 100);
            },
            action: function () {
                this.$emit('action', this.searchInput);
            },
            upperFormat: function (str) {
                return str.toUpperCase();
            },
        },
        mounted() {
            this.$refs.searchInpt.focus();
        },
    }
</script>