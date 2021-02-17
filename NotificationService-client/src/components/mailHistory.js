// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { useBoolean } from '@uifabric/react-hooks';
import {
    SelectionMode, ShimmeredDetailsList, ConstrainMode, Link, ScrollablePane, ScrollbarVisibility, ActionButton,
    Sticky, StickyPositionType, Selection, MarqueeSelection, mergeStyleSets, Stack
} from 'office-ui-fabric-react';
import { getMailHistory, viewMailBody } from "../services";
import ResendModal from './resendModal';
import { CoherencePageSize, CoherencePagination } from '@cseo/controls';
import ViewMailModal from './viewMailModal';
import MailHistoryFilter from './mailHistoryFilter';
const DEFAULT_PAGE_SIZE = 100;
const TOTAL_RECORDS = 10000;
export default function MailHistory() {

    const [hideDialog, { toggle: toggleHideDialog }] = useBoolean(true);

    const [mailDialog, setMailDialog] = useState(true);

    const [showShimmer, setShowShimmer] = useState(true);

    const [mailHistoryData, setMailHistoryData] = useState([]);

    const [nextRecord, setNextRecord] = useState([]);

    const [selectedItem, setSelectedItem] = useState([]);

    const [activeItem, setActiveItem] = useState();

    const [filter, setFilter] = useState([]);

    const [mailBody, setMailBody] = useState();

    const [loader, setLoader] = useState(true);

    const pageCount = React.useRef(0);

    const selectedPage = React.useRef(1);

    const defaultPageSize = React.useRef(DEFAULT_PAGE_SIZE);

    const onPageChange = (newPageNumber) => {
        if (newPageNumber !== selectedPage.current) {
            selectedPage.current = newPageNumber;
            const token = selectedPage.current > 1 ? nextRecord[selectedPage.current - 2] : null;
            fetchMailHistory(filter, token, null);
        }
    };

    const onPageSizeChange = (newPageSize) => {
        defaultPageSize.current = newPageSize;
        selectedPage.current = 1;
        fetchMailHistory(filter, null, defaultPageSize.current);
    }
    const paginationProps = {
        pageCount: pageCount.current,
        selectedPage: selectedPage.current,
        previousPageAriaLabel: 'previous page',
        nextPageAriaLabel: 'next page',
        inputFieldAriaLabel: 'page number',
        onPageChange: onPageChange
    };
    const paginationPageSizeProps = {
        pageSize: defaultPageSize.current,
        pageSizeList: [
            { key: 100, text: '1000' },
            { key: 200, text: '2000' },
            { key: 300, text: '3000' },
            { key: 400, text: '4000' },
            { key: 500, text: '5000' }
        ],
        comboBoxAriaLabel: 'page size',
        onPageSizeChange: onPageSizeChange
    };

    useEffect(() => {
        fetchMailHistory(null, null, null);
    });

    const fetchMailHistory = (filter, token, pageSize) => {
        setShowShimmer(true);
        getMailHistory(token, filter).then(res => {
            setShowShimmer(false);
            setMailHistoryData(res.data);
            nextRecord.push({
                "nextPartitionKey": res.headers['x-nextpartitionkey'],
                "nextRowKey": res.headers['x-nextrowkey']
            });
            setNextRecord(nextRecord);
        }
        ).catch(() => { });
        pageCount.current = Math.ceil(TOTAL_RECORDS / defaultPageSize.current);
    }
    const classNames = mergeStyleSets({
        header: {
            margin: 0,
            backgroundColor: 'white',
        }
    });

    const selection = new Selection({
        onSelectionChanged: () => {
            setSelectedItem(selection.getSelection())
        }
    });

    const columnNames = ["ID", "Subject", "To", "From", "Status", "SentOn", "Error"];

    const fieldNames = ["notificationId", "subject", "to", "from", "status", "sendOnUtcDate", "errorMessage"];

    const columns = columnNames.map((item, index) => (
        {
            key: 'column' + (index + 1),
            name: item,
            fieldName: fieldNames[index],
            minWidth: 80,
            maxWidth: 100,
            isResizable: true

        }));

    const overflowCol = {
        key: 'column',
        name: "",
        fieldName: "Action",
        minWidth: 50,
        maxWidth: 100,
        isResizable: true,
        onRender: () => {
            return <Link
                title="view email"
                aria-label="view email"
                label="View Email"
                onClick={toggleViewMailDialog}
            >View Mail Body</Link>
        },
    };
    columns.push(overflowCol);

    const onRenderDetailsHeader = (props, defaultRender) => {
        if (!props) {
            return null;
        }
        return (
            <Sticky stickyPosition={StickyPositionType.Header} isScrollSynced stickyBackgroundColor="transparent">
                {defaultRender({
                    ...props
                })}
            </Sticky>
        );
    }

    const filterProps = [
        { key: 0, text: "NotificationId", selector: "InputBox", value: [] },
        {
            key: 1, text: "Status", selector: "ComboBox", value: [{ key: 0, text: "Queued" }, { key: 1, text: "Processing" },
            { key: 2, text: "Retrying" }, { key: 3, text: "Failed" }, { key: 4, text: "Sent" },]
        },
        { key: 2, text: "SentOn", selector: "InputBox", value: [] }
    ];

    const operatorItems = [{ key: 0, text: "==" }];

    const onApplyFilter = (obj) => {
        setFilter(obj);
        selectedPage.current = 1;
        fetchMailHistory(obj, null, defaultPageSize.current);
    }

    const onSelectAddFilter = (obj) => { }
    const onSelectFilter = (index, value) => { }
    const onFilterDismiss = (o) => { }
    const onResetFilter = () => {
        selectedPage.current = 1;
        setFilter(undefined);
        fetchMailHistory(null, null, null);
    }
    const onActiveItemChanged = (i, indx, e) => {
        setActiveItem(i);
    }

    const toggleViewMailDialog = () => {
        setLoader(true);
        setMailBody(undefined);
        setMailDialog(!mailDialog);
        if (mailDialog === true && activeItem.status === "Sent") {
            viewMailBody(activeItem?.notificationId).then((response) => {
                setMailBody(response.data.body.content);
                setLoader(false);
            }).catch(error => {
                setLoader(false);
            })
        } else {
            setLoader(false);
        }
    }

    return (
        <div>
            <Sticky >

                <Stack horizontal horizontalAlign="space-between">
                    <span style={{ textAlignLast: "left", paddingLeft: 10 }}>
                        <ActionButton
                            iconProps={{ iconName: 'MailRepeat' }}
                            allowDisabledFocus
                            onClick={toggleHideDialog}>
                            Resend
                        </ActionButton>
                    </span>
                    <span style={{ textAlignLast: "right", paddingRight: 10 }}>
                        <MailHistoryFilter
                            filterItems={filterProps}
                            operatorItems={operatorItems}
                            onApplyFilter={onApplyFilter}
                            onResetFilter={onResetFilter}
                            onSelectAddFilter={onSelectAddFilter}
                            onSelectFilter={onSelectFilter}
                            onFilterDismiss={onFilterDismiss}
                        />
                    </span>
                </Stack>

            </Sticky>
            <div style={{ position: 'relative', height: "76vh" }}>
                <ScrollablePane className={classNames.header} scrollbarVisibility={ScrollbarVisibility.auto}>
                    <MarqueeSelection selection={selection}>
                        <ShimmeredDetailsList
                            setAllSelected='false'
                            selection={selection}
                            setKey='set'
                            enableShimmer={showShimmer}
                            columns={columns}
                            constrainMode={ConstrainMode.unconstrained}
                            items={mailHistoryData ? mailHistoryData : []}
                            isHeaderVisible={true}
                            checkButtonAriaLabel="None"
                            onRenderDetailsHeader={onRenderDetailsHeader}
                            onActiveItemChanged={onActiveItemChanged}
                            selectionMode={SelectionMode.multiple}
                        />
                    </MarqueeSelection>
                </ScrollablePane>
                <ResendModal
                    toggleHideDialog={toggleHideDialog}
                    hideDialog={hideDialog}
                    selectedItem={selectedItem}
                />
                <ViewMailModal
                    toggleMailDialog={toggleViewMailDialog}
                    hideDialog={mailDialog}
                    selectedItem={activeItem}
                    mailBody={mailBody}
                    showLoader={loader}
                />
            </div>
            <Stack horizontal horizontalAlign="end">
                <Stack horizontal horizontalAlign="space-between" styles={{ root: { width: '50%' } }}>
                    <div style={{ marginLeft: -132 }}>
                        <CoherencePagination {...paginationProps} />
                    </div>
                    {/* <CoherencePageSize {...paginationPageSizeProps} /> */}
                </Stack>
            </Stack>
        </div>
    )
}
